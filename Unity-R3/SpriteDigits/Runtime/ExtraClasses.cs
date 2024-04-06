using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace radiants.SpriteDigits
{
	#region Enums

	public enum DisplayMode
	{
		[Tooltip("UnityEngine.SpriteRenderer")]
		Sprite,
		[Tooltip("UnityEngine.UI.Image")]
		Image,
	}

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

	public class DigitsDisplayContainer
	{
		//to toggle SpriteRenderer/Image modes

		public DigitsDisplayContainer(DisplayMode mode, SpriteRenderer spr, UnityEngine.UI.Image image)
		{
			DispMode = mode;
			switch (mode)
			{
				case DisplayMode.Image:
					MyImage = image;
					break;
				case DisplayMode.Sprite:
				default:
					MySpriteRenderer = spr;
					break;
			}
		}

		private SpriteRenderer MySpriteRenderer;
		private UnityEngine.UI.Image MyImage;

		private DisplayMode DispMode;

		public void SetSort(int layerID, int orderInLayer)
		{
			if (DispMode == DisplayMode.Image) return;

			MySpriteRenderer.sortingLayerID = layerID;
			MySpriteRenderer.sortingOrder = orderInLayer;
		}
		public void SetMaterial(Material mat)
		{
			switch (DispMode)
			{
				case DisplayMode.Image:
					MyImage.material = mat;
					return;
				case DisplayMode.Sprite:
				default:
					if (mat == null) mat = DefaultMaterialUtil.DefaultSpriteMaterial;
					MySpriteRenderer.sharedMaterial = mat;
					return;
			}
		}

		public GameObject gameObject
		{
			get
			{
				switch (DispMode)
				{
					case DisplayMode.Image:
						return MyImage.gameObject;
					case DisplayMode.Sprite:
					default:
						return MySpriteRenderer.gameObject;
				}
			}
		}

		public Transform transform
		{
			get
			{
				switch (DispMode)
				{
					case DisplayMode.Image:
						return MyImage.transform;
					case DisplayMode.Sprite:
					default:
						return MySpriteRenderer.transform;
				}
			}
		}

		public bool enabled
		{
			get
			{
				switch (DispMode)
				{
					case DisplayMode.Image:
						return MyImage.enabled;
					case DisplayMode.Sprite:
					default:
						return MySpriteRenderer.enabled;
				}
			}
			set
			{
				switch (DispMode)
				{
					case DisplayMode.Image:
						MyImage.enabled = value;
						return;
					case DisplayMode.Sprite:
					default:
						MySpriteRenderer.enabled = value;
						return;
				}
			}
		}
		public Sprite sprite
		{
			get
			{
				switch (DispMode)
				{
					case DisplayMode.Image:
						return MyImage.sprite;
					case DisplayMode.Sprite:
					default:
						return MySpriteRenderer.sprite;
				}
			}
			set
			{
				switch (DispMode)
				{
					case DisplayMode.Image:
						MyImage.sprite = value;
						MyImage.rectTransform.sizeDelta = value.bounds.size;
						return;
					case DisplayMode.Sprite:
					default:
						MySpriteRenderer.sprite = value;
						return;
				}
			}
		}
		public Color color
		{
			get
			{
				switch (DispMode)
				{
					case DisplayMode.Image:
						return MyImage.color;
					case DisplayMode.Sprite:
					default:
						return MySpriteRenderer.color;
				}
			}
			set
			{
				switch (DispMode)
				{
					case DisplayMode.Image:
						MyImage.color = value;
						return;
					case DisplayMode.Sprite:
					default:
						MySpriteRenderer.color = value;
						return;
				}
			}
		}
	}


	public static class DefaultMaterialUtil
	{
		public static Material DefaultSpriteMaterial { get; private set; } = null;
		static DefaultMaterialUtil()
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
	}

}