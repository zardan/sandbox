﻿using Mellis;
using Mellis.Core.Interfaces;
using UnityEngine;

namespace Zifro.Sandbox.ClrFunctions
{
	public class IsBlockedFunction : ClrFunction
	{
		readonly Direction direction;
		readonly PlayerController player;
		readonly GridWorld world;

		public IsBlockedFunction(string name, Direction direction) : base(name)
		{
			this.direction = direction;
			player = Object.FindObjectOfType<PlayerController>();
			world = Object.FindObjectOfType<GridWorld>();
			
			Debug.Assert(player, "Unable to find " + nameof(PlayerController));
			Debug.Assert(world, "Unable to find " + nameof(GridWorld));
		}

		public override IScriptType Invoke(params IScriptType[] arguments)
		{
			if (arguments.Length <= 0 || !arguments[0].TryConvert(out float scale))
			{
				scale = 1;
			}

			Vector3 point = player.fractionPosition;

			if (scale.Equals(0f))
			{
				return Processor.Factory.Create(world.IsPointInBlock(point));
			}

			Vector3 vector = player.GetDirectionFraction(direction, scale);
			bool isBlocked = world.TryRaycastBlocks(point, vector, scale, out RaycastHit _);

			return Processor.Factory.Create(isBlocked);
		}
	}
}
