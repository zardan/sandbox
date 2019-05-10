﻿using UnityEngine;
using UnityEngine.EventSystems;
using Zifro.Sandbox.Entities;

namespace Zifro.Sandbox.UI.WorldEdit
{
	public class ToolCamera : WorldEditTool,
		IToolScreenEnter,
		IToolScreenExit
	{
		public Transform gameCameraArm;

		[Space]
		public float minZoomIn = 2;
		public float maxZoomOut = 10;

		[Space]
		public int tiltMin;
		public int tiltMax = 90;
		public bool tiltRound = true;
		public float tiltSpeed = -2f;

		[Space]
		public int rotationMin = -90;
		public int rotationMax;
		public bool rotationRound = true;
		public float rotationSpeed = 3f;

		[Space]
		public float dragZoomFactor = 1 / 10f;

		[SerializeField, HideInInspector]
		float rotation;
		[SerializeField, HideInInspector]
		float tilt;

		bool pointerOver;

		bool dragging;
		bool rotating;

		void Awake()
		{
			Debug.Assert(gameCameraArm, $"{nameof(gameCameraArm)} is not assigned.", this);

			rotation = Mathf.DeltaAngle(0, gameCameraArm.localEulerAngles.y);
			tilt = Mathf.DeltaAngle(0, gameCameraArm.localEulerAngles.x);
		}

		void OnValidate()
		{
			minZoomIn = Mathf.Clamp(minZoomIn, 0, maxZoomOut);
			maxZoomOut = Mathf.Max(maxZoomOut, minZoomIn);
		}

		void Update()
		{
			float zoom = gameCamera.orthographicSize + Input.mouseScrollDelta.y;
			gameCamera.orthographicSize = Mathf.Clamp(zoom, minZoomIn, maxZoomOut);

			Drag();
			Rotate();
		}

		private void Drag()
		{
			if (Input.GetMouseButtonDown(0) && !dragging)
			{
				dragging = true;
			}
			else if (dragging)
			{
				if (!Input.GetMouseButton(0))
				{
					dragging = false;
					enabled = pointerOver && isSelected;
				}
				else
				{
					float zoomFactor = gameCamera.orthographicSize * dragZoomFactor;

					float dx = Input.GetAxis("Mouse X") * zoomFactor;
					float dy = Input.GetAxis("Mouse Y") * zoomFactor;

					gameCameraArm.Translate(dx, dy, 0, Space.Self);
				}
			}
		}

		private void Rotate()
		{
			if (Input.GetMouseButtonDown(1) && !rotating)
			{
				//Vector3 from = gameCamera.ScreenToWorldPoint(Input.mousePosition);
				//Vector3 rayDirection = gameCamera.transform.forward;

				//if (world.TryRaycastBlocks(from, rayDirection, gameCamera.farClipPlane, out GridRaycastHit hit))
				//{
				//	rotating = true;
				//	grabDistance = hit.distance;
				//	Debug.DrawRay(from, rayDirection * grabDistance, Color.yellow, 1f);

				//	Vector3 camPosition = gameCamera.transform.position;
				//	gameCameraArm.position = hit.point;
				//	gameCamera.transform.position = camPosition;
				//}
				//else if (world.groundPlane.Raycast(new Ray(from, rayDirection), out float rayDistance))
				//{
				//	rotating = true;
				//	grabDistance = rayDistance;
				//	Debug.DrawRay(from, rayDirection * grabDistance, Color.cyan, 1f);

				//	Vector3 camPosition = gameCamera.transform.position;
				//	gameCameraArm.position = from + rayDirection * grabDistance;
				//	gameCamera.transform.position = camPosition;
				//}
				//else
				//{
				//	Debug.DrawRay(from, rayDirection * gameCamera.farClipPlane, Color.gray, 1f);
				//}

				rotating = true;
			}
			else if (rotating)
			{
				if (!Input.GetMouseButton(1))
				{
					rotating = false;
					enabled = pointerOver && isSelected;
				}
				else
				{
					float dx = Input.GetAxis("Mouse X") * rotationSpeed;
					float dy = Input.GetAxis("Mouse Y") * tiltSpeed;

					rotation = Mathf.Clamp(rotation + dx, rotationMin, rotationMax);
					tilt = Mathf.Clamp(tilt + dy, tiltMin, tiltMax);

					gameCameraArm.localEulerAngles = new Vector3(
						tiltRound ? Mathf.Round(tilt) : tilt,
						rotationRound ? Mathf.Round(rotation) : rotation,
						0
					);
				}
			}
		}

		public override void OnToolSelected()
		{
			enabled = pointerOver && isSelected;
			rotating = false;
		}

		public override void OnToolDeselected()
		{
			rotating = false;
			enabled = false;
		}

		void IToolScreenEnter.OnScreenEnter(PointerEventData eventData)
		{
			pointerOver = true;
			enabled = pointerOver && isSelected;
		}

		void IToolScreenExit.OnScreenExit(PointerEventData eventData)
		{
			pointerOver = false;
			enabled = (rotating || dragging) && isSelected;
		}
	}
}