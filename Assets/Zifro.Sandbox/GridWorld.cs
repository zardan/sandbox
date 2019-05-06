﻿using System.Diagnostics.Contracts;
using System.Linq;
using UnityEngine;
using Zifro.Sandbox;

public class GridWorld : MonoBehaviour
{
	public GameObject blockPrefab;

	[TextArea]
	public string[] initializationLayers;

	bool[,,] grid;

	void Awake()
	{
		RegenerateGrid();
	}

//#if UNITY_EDITOR
//	public bool regenerate;
//	void OnValidate()
//	{
//		if (blockPrefab && regenerate)
//		{
//			regenerate = false;
//			RegenerateGrid();
//		}
//	}
//#endif

	private void RegenerateGrid()
	{
		// Remove old
		foreach (Transform child in transform)
		{
//#if UNITY_EDITOR
//			if (!EditorApplication.isPlaying)
//			{
//				DestroyImmediate(child.gameObject);
//				continue;
//			}
//#endif
			Destroy(child.gameObject);
		}

		// Add new
		string[][] splitByLine = initializationLayers.Select(o => o.Split('\n')).ToArray();

		int height = splitByLine.Length;
		int width = splitByLine.Max(o => o.Length);
		int length = splitByLine.Max(o => o.Max(n => n.Length));

		grid = new bool[width, height, length];
		Transform parent = transform;

		for (int y = 0; y < splitByLine.Length; y++)
		{
			string[] layer = splitByLine[y];
			for (int x = 0; x < layer.Length; x++)
			{
				string line = layer[x];
				for (int z = 0; z < line.Length; z++)
				{
					if (!char.IsLetterOrDigit(line[z]))
					{
						continue;
					}

					grid[x, y, z] = true;
					Instantiate(blockPrefab,
						parent.TransformPoint(x + 0.5f, y + 0.5f, z + 0.5f),
						Quaternion.identity,
						parent);
				}
			}
		}
	}

	[Pure]
	public bool IsPointInBlock(float x, float y, float z)
	{
		Vector3 localPoint = transform.InverseTransformPoint(x, y, z);
		return IsLocalPointInBlock(localPoint);
	}

	[Pure]
	public bool IsPointInBlock(Vector3 point)
	{
		Vector3 localPoint = transform.InverseTransformPoint(point);
		return IsLocalPointInBlock(localPoint);
	}

	[Pure]
	private bool IsLocalPointInBlock(Vector3 localPoint)
	{
		if (grid == null)
		{
			return false;
		}

		if (localPoint.x >= 0 && localPoint.x < grid.GetLength(0) &&
		    localPoint.y >= 0 && localPoint.y < grid.GetLength(1) &&
		    localPoint.z >= 0 && localPoint.z < grid.GetLength(2))
		{
			return grid[(int)localPoint.x, (int)localPoint.y, (int)localPoint.z];
		}

		return false;
	}

	[Pure]
	private bool IsLocalPointInBlock(Vector3Int localPoint)
	{
		if (grid == null)
		{
			return false;
		}

		if (localPoint.x >= 0 && localPoint.x < grid.GetLength(0) &&
		    localPoint.y >= 0 && localPoint.y < grid.GetLength(1) &&
		    localPoint.z >= 0 && localPoint.z < grid.GetLength(2))
		{
			return grid[localPoint.x, localPoint.y, localPoint.z];
		}

		return false;
	}

	[Pure]
	public bool TryRaycastBlocks(Vector3 pointWorld, Vector3 directionWorld, float maxLength, out RaycastHit hit)
	{
		Vector3 pointLocal = transform.InverseTransformPoint(pointWorld);
		Vector3 directionLocal = transform.InverseTransformDirection(directionWorld);

		return TryRaycastBlocksLocal(pointLocal, directionLocal, maxLength, out hit);
	}

	[Pure]
	private bool TryRaycastBlocksLocal(Vector3 pointLocal, Vector3 directionLocal, float maxLength, out RaycastHit hit)
	{
		Debug.Assert(!directionLocal.Equals(Vector3.zero), "Cannot raycast along zero vector.");

		float t = 0;

		var intPointLocal = new Vector3Int((int)pointLocal.x, (int)pointLocal.y, (int)pointLocal.z);

		var step = new Vector3Int(
			directionLocal.x > 0 ? 1 : -1,
			directionLocal.y > 0 ? 1 : -1,
			directionLocal.z > 0 ? 1 : -1
		);

		Vector3 normalizedDirection = directionLocal.normalized;

		var tDelta = new Vector3(
			Mathf.Abs(1 / normalizedDirection.x),
			Mathf.Abs(1 / normalizedDirection.y),
			Mathf.Abs(1 / normalizedDirection.z)
		);

		var dist_RENAME_ME = new Vector3(
			step.x > 0 ? intPointLocal.x + 1 - pointLocal.x : pointLocal.x - intPointLocal.x,
			step.y > 0 ? intPointLocal.y + 1 - pointLocal.y : pointLocal.y - intPointLocal.y,
			step.z > 0 ? intPointLocal.z + 1 - pointLocal.z : pointLocal.z - intPointLocal.z
		);

		// Nearest voxel boundary
		var tMax = new Vector3(
			tDelta.x < float.PositiveInfinity ? tDelta.x * dist_RENAME_ME.x : float.PositiveInfinity,
			tDelta.y < float.PositiveInfinity ? tDelta.y * dist_RENAME_ME.y : float.PositiveInfinity,
			tDelta.z < float.PositiveInfinity ? tDelta.z * dist_RENAME_ME.z : float.PositiveInfinity
		);

		int steppedIndex = -1;

		while (t <= maxLength)
		{
			bool block = IsLocalPointInBlock(intPointLocal);

//#if UNITY_EDITOR
//			Vector3 currentPointLocal = pointLocal + t * directionLocal;
//			Vector3 currentPointWorld = transform.TransformPoint(currentPointLocal);
//			Gizmos.DrawWireSphere(currentPointWorld, 0.05f);
//			var currentNormalLocal = new Vector3(
//				steppedIndex == 0 ? -step.x : 0,
//				steppedIndex == 1 ? -step.y : 0,
//				steppedIndex == 2 ? -step.z : 0
//			);
//			Vector3 currentNormalWorld = transform.TransformDirection(currentNormalLocal);
//			Gizmos.DrawRay(currentPointWorld, currentNormalWorld * 0.3f);
//			UnityEditor.Handles.Label(currentPointWorld, intPointLocal.ToString());
//#endif

			if (block)
			{
				Vector3 hitPointLocal = pointLocal + t * directionLocal;
				Vector3 hitPointWorld = transform.TransformPoint(hitPointLocal);

				var hitNormalLocal = new Vector3(
					steppedIndex == 0 ? -step.x : 0,
					steppedIndex == 1 ? -step.y : 0,
					steppedIndex == 2 ? -step.z : 0
				);
				Vector3 hitNormalWorld = transform.TransformDirection(hitNormalLocal);

				hit = new RaycastHit {
					distance = t,
					normal = hitNormalWorld,
					point = hitPointWorld
				};

				return true;
			}

			// Advance t to next nearest voxel boundary
			if (tMax.x < tMax.y)
			{
				if (tMax.x < tMax.z)
				{
					intPointLocal.x += step.x;
					t = tMax.x;
					tMax.x += tDelta.x;
					steppedIndex = 0;
				}
				else
				{
					intPointLocal.z += step.z;
					t = tMax.z;
					tMax.z += tDelta.z;
					steppedIndex = 2;
				}
			}
			else
			{
				if (tMax.y < tMax.z)
				{
					intPointLocal.y += step.y;
					t = tMax.y;
					tMax.y += tDelta.y;
					steppedIndex = 1;
				}
				else
				{
					intPointLocal.z += step.z;
					t = tMax.z;
					tMax.z += tDelta.z;
					steppedIndex = 2;
				}
			}
		}

		// no voxel hit found
		Vector3 lastPointLocal = pointLocal + t * directionLocal;
		Vector3 lastPointWorld = transform.TransformPoint(lastPointLocal);

		hit = new RaycastHit {
			point = lastPointWorld,
			normal = Vector3.zero
		};

		return false;
	}
}