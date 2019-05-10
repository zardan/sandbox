﻿using System;
using System.Collections.Generic;
using System.Linq;
using PM;
using UnityEngine;
using UnityEngine.UI;
using Zifro.Sandbox.UI.WorldEdit;

namespace Zifro.Sandbox.UI
{
	public class WorldEditToolsSelector : MonoBehaviour,
		IPMCompilerStarted,
		IPMCompilerStopped
	{
		public VariableWindow variableWindow;
		public GridWorld gridWorld;

		[SerializeField, HideInInspector]
		WorldEditTool[] _tools;

		public WorldEditTool currentTool { get; private set; }
		public IReadOnlyList<WorldEditTool> tools => _tools;

		void Awake()
		{
			_tools = GetComponentsInChildren<WorldEditTool>();

			Debug.Assert(_tools.Length > 0, "No tools found.", this);
			Debug.Assert(variableWindow, "Variable window not defined.", this);

			Debug.Assert(gridWorld, $"{nameof(gridWorld)} not assigned.", this);
		}

		void OnEnable()
		{
			Debug.Assert(Camera.main, "Missing main camera!.", this);

			foreach (WorldEditTool tool in _tools)
			{
				// Initialize tool
				tool.button = tool.GetComponent<Button>();
				Debug.Assert(tool.button, $"Did not find button for tool {tool.GetType().Name} in \"{tool.name}\"", tool);

				tool.world = gridWorld;
				tool.gameCamera = Camera.main;

				// Register events
				tool.button.onClick.AddListener(() => SelectTool(tool));
			}

			SelectTool(_tools.First());
		}

		void Update()
		{
			foreach (WorldEditTool tool in _tools)
			{
				if (!tool.isSelected && Input.GetKeyDown(tool.hotKey))
				{
					SelectTool(tool);
					break;
				}
			}
		}

		public void SelectTool(WorldEditTool selectThis)
		{
			if (currentTool == selectThis)
			{
				return;
			}

			foreach (WorldEditTool tool in _tools)
			{
				if (tool == selectThis)
				{
					continue;
				}

				if (!tool.isSelected)
				{
					continue;
				}

				tool.button.interactable = true;
				tool.isSelected = false;
				tool.OnToolDeselected();
			}

			currentTool = selectThis;
			currentTool.button.interactable = false;
			currentTool.isSelected = true;
			currentTool.OnToolSelected();

			variableWindow.gameObject.SetActive(false);
		}

		public void DeselectTool()
		{
			if (!currentTool)
			{
				return;
			}

			currentTool.button.interactable = true;
			currentTool.isSelected = false;
			currentTool.OnToolDeselected();
			currentTool = null;

			variableWindow.gameObject.SetActive(true);
		}

		void IPMCompilerStarted.OnPMCompilerStarted()
		{
			DeselectTool();
			foreach (WorldEditTool tool in _tools)
			{
				tool.button.interactable = false;
			}

			enabled = false;
		}

		void IPMCompilerStopped.OnPMCompilerStopped(StopStatus status)
		{
			foreach (WorldEditTool tool in _tools)
			{
				tool.button.interactable = true;
			}

			enabled = true;
		}
	}
}