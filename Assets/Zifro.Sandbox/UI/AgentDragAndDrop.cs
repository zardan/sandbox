﻿using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zifro.Sandbox.Entities;
using Zifro.Sandbox.UI.WorldEdit;

namespace Zifro.Sandbox.UI
{
	public class AgentDragAndDrop : WorldEditTool,
		IBeginDragHandler,
		IDragHandler,
		IEndDragHandler,
		IPointerClickHandler
	{
		public WorldEditToolsList toolsList;
		public string placeInput = "Fire1";
		public string cancelInput = "Cancel";
		public Material dragMaterial;
		public Text agentLabel;
		public RawImage agentPreviewImage;

		PlacementMode placeState = PlacementMode.None;
		WorldEditTool lastTool;
		GameObject draggedGhost;
		Agent draggedAgent;
		bool isActivatingClickAndDragThisFrame;

		enum PlacementMode
		{
			None,
			DragAndDrop,
			ClickAndPlace
		}

		void Start()
		{
			Debug.Assert(toolsList, $"{nameof(toolsList)} not defined in {name}.", this);
			Debug.Assert(agentLabel, $"{nameof(agentLabel)} not defined in {name}.", this);
			Debug.Assert(agentPreviewImage, $"{nameof(agentPreviewImage)} not defined in {name}.", this);

			if (draggedAgent == null)
			{
				HideTool();
			}
		}

		void Update()
		{
			if (placeState == PlacementMode.None || !isMouseOverGame)
			{
				return;
			}

			if (Input.GetButtonDown(cancelInput))
			{
				// Cancel drag
				DragEndOrCancel();
				return;
			}

			Vector3 point = gameCamera.ScreenToWorldPoint(Input.mousePosition);

			if (world.TryRaycastBlocks(point, gameCamera.transform.forward, gameCamera.farClipPlane,
				out GridRaycastHit hit))
			{
				draggedGhost.transform.position = world.VoxelToWorld(hit.voxelIndex + hit.voxelNormal);
				draggedGhost.gameObject.SetActive(true);

				if (placeState == PlacementMode.ClickAndPlace && Input.GetButtonDown(placeInput))
				{
					EndPlacement();
					DragEndOrCancel();
				}
			}
			else
			{
				draggedGhost.gameObject.SetActive(false);
			}
		}

		void OnDisable()
		{
			if (draggedGhost)
			{
				Destroy(draggedGhost);
				draggedGhost = null;
			}
		}

		void DragEndOrCancel()
		{
			if (!toolsList.isSelecting)
			{
				if (lastTool)
				{
					// Switch to last used tool
					toolsList.SelectTool(lastTool);
				}
				else if (placeState == PlacementMode.ClickAndPlace)
				{
					// Deselect self
					toolsList.DeselectTool();
				}
			}

			lastTool = null;
			placeState = PlacementMode.None;

			if (draggedGhost)
			{
				Destroy(draggedGhost);
				draggedGhost = null;
			}
		}

		void StartPlacement()
		{
			draggedGhost = Instantiate(draggedAgent.modelPrefab);

			if (dragMaterial)
			{
				foreach (Renderer child in draggedGhost.GetComponentsInChildren<Renderer>())
				{
					child.sharedMaterial = dragMaterial;
				}
			}
		}

		void EndPlacement()
		{
			Vector3 position = draggedGhost.transform.position;
			GameObject clone = Instantiate(AgentBank.main.agentPrefab, position, draggedGhost.transform.rotation, AgentBank.main.transform);
			Instantiate(draggedAgent.modelPrefab, clone.transform.position, clone.transform.rotation, clone.transform);

			AgentInstance agentInstance = clone.GetComponent<AgentInstance>();
			agentInstance.fractionPosition = (FractionVector3)position;

			draggedAgent.instances.Add(agentInstance);
		}

		public void ShowTool(Agent agent)
		{
			Debug.Assert(ModelPreviewBank.main, "No main model preview bank registered.", this);

			draggedAgent = agent;
			agentLabel.text = agent.name;
			gameObject.SetActive(true);
			agentPreviewImage.texture = ModelPreviewBank.main.GetOrCreateTexture(agent.modelPrefab);
		}

		public void HideTool()
		{
			DragEndOrCancel();
			draggedAgent = null;
			gameObject.SetActive(false);
		}

		void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
		{
			if (placeState != PlacementMode.None || draggedAgent == null)
			{
				// Stop drag events
				eventData.pointerDrag = null;
				return;
			}

			lastTool = toolsList.currentTool;

			if (lastTool)
			{
				toolsList.DeselectToolWithoutUIUpdate();
			}

			placeState = PlacementMode.DragAndDrop;
			StartPlacement();
		}

		void IDragHandler.OnDrag(PointerEventData eventData)
		{
			if (placeState != PlacementMode.DragAndDrop)
			{
				// Stop drag events
				eventData.pointerDrag = null;
			}
		}

		void IEndDragHandler.OnEndDrag(PointerEventData eventData)
		{
			if (placeState != PlacementMode.DragAndDrop)
			{
				return;
			}

			// Release the item, successful end drag

			Vector3 point = gameCamera.ScreenToWorldPoint(Input.mousePosition);

			if (world.TryRaycastBlocks(point, gameCamera.transform.forward, gameCamera.farClipPlane,
				out GridRaycastHit hit))
			{
				draggedGhost.transform.position = world.VoxelToWorld(hit.voxelIndex + hit.voxelNormal);

				EndPlacement();
			}

			DragEndOrCancel();
		}

		void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
		{
			if (isSelected && 
			    placeState == PlacementMode.ClickAndPlace &&
			    !isActivatingClickAndDragThisFrame)
			{
				DragEndOrCancel();
			}
		}

		IEnumerator DisableClickAndDragBoolNextFrame()
		{
			yield return null;
			isActivatingClickAndDragThisFrame = false;
		}

		public override void OnToolSelectedChange(WorldEditTool last)
		{
			if (isSelected && placeState == PlacementMode.None)
			{
				placeState = PlacementMode.ClickAndPlace;
				StartPlacement();
				lastTool = last;
				button.interactable = true;
				isActivatingClickAndDragThisFrame = true;
				StartCoroutine(DisableClickAndDragBoolNextFrame());
				EventSystem.current.SetSelectedGameObject(gameObject);
			}
			else
			{
				DragEndOrCancel();
			}
		}

		public override void OnMouseOverChange()
		{
		}
	}
}
