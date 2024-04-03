using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace radiants.SpriteDigits
{
	#region Enums

	public enum PaddingMode
	{
		Pad,
		ZeroFill,
	}

	public enum HorizontalPivot
	{
		Left,
		Center,
		Right,
	}

	public enum VerticalPivot
	{
		Top,
		Center,
		Bottom,
	}

	#endregion




	public static class DefaultSpriteMaterialUtil
	{
		public static Material DefaultSpriteMaterial { get; private set; } = null;

		static DefaultSpriteMaterialUtil()
		{
			var allMats = Resources.FindObjectsOfTypeAll<Material>();

			foreach (var mat in allMats)
			{
				if (mat.name == "Sprites-Default")
				{
					DefaultSpriteMaterial = mat;
					return;
				}
			}
			allMats = null;
			Resources.UnloadUnusedAssets();
		}

		/* Not work on Unity 2021.x
		#if UNITY_EDITOR
					return UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
		#else
					return Resources.GetBuiltinResource<Material>("Sprites-Default.mat");
		#endif
		*/
	}



}