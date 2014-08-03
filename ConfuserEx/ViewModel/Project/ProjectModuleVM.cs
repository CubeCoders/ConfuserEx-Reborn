﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;
using Confuser.Core.Project;

namespace ConfuserEx.ViewModel {
	public class ProjectModuleVM : ViewModelBase, IViewModel<ProjectModule>, IRuleContainer {

		private readonly ProjectModule module;
		private readonly ProjectVM parent;
		private string asmName = "Unknown";
		private string simpleName;

		public ProjectModuleVM(ProjectVM parent, ProjectModule module) {
			this.parent = parent;
			this.module = module;

			ObservableCollection<ProjectRuleVM> rules = Utils.Wrap(module.Rules, rule => new ProjectRuleVM(parent, rule));
			rules.CollectionChanged += (sender, e) => parent.IsModified = true;
			Rules = rules;

			if (module.Path != null) {
				SimpleName = System.IO.Path.GetFileName(module.Path);
				LoadAssemblyName();
			}
		}

		public ProjectModule Module {
			get { return module; }
		}

		public string Path {
			get { return module.Path; }
			set {
				if (SetProperty(module.Path != value, val => module.Path = val, value, "Path")) {
					parent.IsModified = true;
					SimpleName = System.IO.Path.GetFileName(module.Path);
					LoadAssemblyName();
				}
			}
		}

		public string SimpleName {
			get { return simpleName; }
			private set { SetProperty(ref simpleName, value, "SimpleName"); }
		}

		public string AssemblyName {
			get { return asmName; }
			private set { SetProperty(ref asmName, value, "AssemblyName"); }
		}

		public string SNKeyPath {
			get { return module.SNKeyPath; }
			set {
				if (SetProperty(module.SNKeyPath != value, val => module.SNKeyPath = val, value, "SNKeyPath"))
					parent.IsModified = true;
			}
		}

		public string SNKeyPassword {
			get { return module.SNKeyPassword; }
			set {
				if (SetProperty(module.SNKeyPassword != value, val => module.SNKeyPassword = val, value, "SNKeyPassword"))
					parent.IsModified = true;
			}
		}

		public IList<ProjectRuleVM> Rules { get; private set; }

		ProjectModule IViewModel<ProjectModule>.Model {
			get { return module; }
		}

		private void LoadAssemblyName() {
			AssemblyName = "Loading...";
			ThreadPool.QueueUserWorkItem(_ => {
				try {
					string path = System.IO.Path.Combine(parent.BaseDirectory, Path);
					AssemblyName name = System.Reflection.AssemblyName.GetAssemblyName(path);
					AssemblyName = name.FullName;
				}
				catch {
					AssemblyName = "Unknown";
				}
			});
		}

	}
}