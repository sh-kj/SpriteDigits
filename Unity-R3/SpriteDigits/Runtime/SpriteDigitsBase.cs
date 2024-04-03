﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using R3;

namespace radiants.SpriteDigits
{
	[RequireComponent(typeof(RectTransform))]
	public abstract class SpriteDigitsBase : MonoBehaviour
	{
		#region Serialize/Observables

		protected Subject<Unit> RectTransformDimensionsChangeSubject = new Subject<Unit>();

		[SerializeField]
		protected SerializableReactiveProperty<Digits> _Digits = new SerializableReactiveProperty<Digits>();
		public Digits Digits
		{
			get { return _Digits.Value; }
			set { _Digits.Value = value; }
		}

		[SerializeField]
		protected SerializableReactiveProperty<Color> _Color = new SerializableReactiveProperty<Color>(Color.white);
		public Color Color
		{
			get { return _Color.Value; }
			set { _Color.Value = value; }
		}

		[SerializeField]
		protected SerializableReactiveProperty<int> _SortingLayerID = new SerializableReactiveProperty<int>(0);
		public int SortingLayerID
		{
			get { return _SortingLayerID.Value; }
			set { _SortingLayerID.Value = value; }
		}
		[SerializeField]
		protected SerializableReactiveProperty<int> _OrderInLayer = new SerializableReactiveProperty<int>(0);
		public int OrderInLayer
		{
			get { return _OrderInLayer.Value; }
			set { _OrderInLayer.Value = value; }
		}

		[SerializeField]
		protected SerializableReactiveProperty<Material> _CustomMaterial = new SerializableReactiveProperty<Material>();
		public Material CustomMaterial
		{
			get { return _CustomMaterial.Value; }
			set { _CustomMaterial.Value = value; }
		}

		[SerializeField]
		protected SerializableReactiveProperty<float> _Size = new SerializableReactiveProperty<float>(50f);
		public float Size
		{
			get { return _Size.Value; }
			set { _Size.Value = value; }
		}

		[SerializeField]
		protected SerializableReactiveProperty<float> _Spacing = new SerializableReactiveProperty<float>(0f);
		public float Spacing
		{
			get { return _Spacing.Value; }
			set { _Spacing.Value = value; }
		}

		[SerializeField]
		protected SerializableReactiveProperty<HorizontalPivot> _HorizontalPivot = new SerializableReactiveProperty<HorizontalPivot>(HorizontalPivot.Center);
		public HorizontalPivot HorizontalPivot
		{
			get { return _HorizontalPivot.Value; }
			set { _HorizontalPivot.Value = value; }
		}

		[SerializeField]
		protected SerializableReactiveProperty<VerticalPivot> _VerticalPivot = new SerializableReactiveProperty<VerticalPivot>(VerticalPivot.Center);
		public VerticalPivot VerticalPivot
		{
			get { return _VerticalPivot.Value; }
			set { _VerticalPivot.Value = value; }
		}

		protected RectTransform _MyRectTransform;
		protected RectTransform MyRectTransform
		{
			get
			{
				if (_MyRectTransform == null) _MyRectTransform = GetComponent<RectTransform>();
				return _MyRectTransform;
			}
		}

		#endregion

		#region Observables

		private void SubscribeObservablesBasic()
		{
			RectTransformDimensionsChangeSubject
				.Subscribe(_ => ApplyNumbers())
				.AddTo(Disposables);

			_Digits.Subscribe(_ => ApplyNumbers()).AddTo(Disposables);
			_Size.Subscribe(_ => ApplyNumbers()).AddTo(Disposables);
			_Spacing.Subscribe(_ => ApplyNumbers()).AddTo(Disposables);
			_HorizontalPivot.Subscribe(_ => ApplyNumbers()).AddTo(Disposables);
			_VerticalPivot.Subscribe(_ => ApplyNumbers()).AddTo(Disposables);

			_CustomMaterial.Subscribe(_mat => SetMaterialToRenderers(GetMaterialForSprite(_mat)))
				.AddTo(Disposables);

			_Color.Subscribe(_col => SetColorToRenderers(_col))
				.AddTo(Disposables);

			_SortingLayerID.Subscribe(_ => SetSortToRenderers(SortingLayerID, OrderInLayer))
				.AddTo(Disposables);
			_OrderInLayer.Subscribe(_ => SetSortToRenderers(SortingLayerID, OrderInLayer))
				.AddTo(Disposables);

			SubscribeObservables();
		}

		protected abstract void SubscribeObservables();

		protected CompositeDisposable Disposables = new CompositeDisposable();

		#endregion

		#region Undo/Redo

#if UNITY_EDITOR
		//support undo/redo
		private void PerformUndoRedo()
		{
			if (this.enabled)
			{
				Refresh();
			}
		}
		private void Awake()
		{
			UnityEditor.Undo.undoRedoPerformed += PerformUndoRedo;
		}
#endif
		private void UnsbscribeUndo()
		{
#if UNITY_EDITOR
			UnityEditor.Undo.undoRedoPerformed -= PerformUndoRedo;
#endif
		}

		/// <summary>
		/// In most cases, you don't need to call this.
		/// </summary>
		public virtual void Refresh()
		{
			PrepareRenderers();
			ApplyNumbers();

			SetMaterialToRenderers(GetMaterialForSprite(CustomMaterial));
			SetColorToRenderers(this.Color);
			SetSortToRenderers(SortingLayerID, OrderInLayer);
		}


		#endregion

		#region Monobehaviour callbacks

		private void OnEnable()
		{
			PrepareRenderers();
			SubscribeObservablesBasic();
		}

		private void OnDisable()
		{
			Disposables.Clear();
			DisableAllRenderers();
		}

		private void OnDestroy()
		{
			Disposables.Dispose();
			DestroyAllRenderers();
			UnsbscribeUndo();
		}

		protected void DestroyObject(GameObject obj)
		{
			if (Application.isPlaying)
				Destroy(obj);
			else
				DestroyImmediate(obj, false);
		}

		private void OnRectTransformDimensionsChange()
		{
			RectTransformDimensionsChangeSubject.OnNext(Unit.Default);
		}

		#endregion

		#region Renderer Management

		protected abstract void PrepareRenderers();

		protected SpriteRenderer CreateChildRenderer()
		{
			GameObject child = new GameObject("");
			//debug
			//child.hideFlags = HideFlags.DontSave;
			child.hideFlags = HideFlags.HideAndDontSave;
			child.transform.SetParent(transform);
			child.transform.localRotation = Quaternion.identity;
			child.layer = gameObject.layer;
			var spr = child.AddComponent<SpriteRenderer>();
			spr.enabled = false;

			//set current material/color/sort values
			spr.sharedMaterial = GetMaterialForSprite(CustomMaterial);
			spr.color = Color;
			spr.sortingLayerID = SortingLayerID;
			spr.sortingOrder = OrderInLayer;
			return spr;
		}

		protected abstract void DestroyAllRenderers();

		protected void DisableAllRenderers()
		{
			ActForAllRenderers(_rend => _rend.enabled = false);
		}

		protected void SetMaterialToRenderers(Material mat)
		{
			ActForAllRenderers(_rend => _rend.sharedMaterial = mat);
		}

		protected void SetColorToRenderers(Color col)
		{
			ActForAllRenderers(_rend => _rend.color = col);
		}

		protected void SetSortToRenderers(int layerID, int order)
		{
			ActForAllRenderers(_rend => 
			{
				_rend.sortingLayerID = layerID;
				_rend.sortingOrder = order;
			});
		}

		protected abstract void ActForAllRenderers(System.Action<SpriteRenderer> action);


		#endregion

		#region Apply Numbers

		protected abstract void ApplyNumbers();


		protected Vector3 GetPivotOrigin(HorizontalPivot horizontal, VerticalPivot vertical, Rect rect, float scaledWidth)
		{
			Vector2 pivot = MyRectTransform.pivot;
			Vector3 origin = new Vector3();
			switch (horizontal)
			{
				case HorizontalPivot.Left:
					origin.x = rect.xMin + scaledWidth;
					break;
				case HorizontalPivot.Right:
					origin.x = rect.xMax;
					break;
				case HorizontalPivot.Center:
				default:
					origin.x = scaledWidth / 2f - rect.width * (pivot.x - 0.5f);
					break;
			}
			switch (vertical)
			{
				case VerticalPivot.Top:
					origin.y = rect.yMax;
					break;
				case VerticalPivot.Bottom:
					origin.y = rect.yMin;
					break;

				case VerticalPivot.Center:
				default:
					origin.y = -rect.height * (pivot.y - 0.5f);
					break;
			}

			return origin;
		}

		protected static void SetRendererPosition(ref Vector3 caret, Transform trans, HorizontalPivot horizontal, VerticalPivot vertical,
			Bounds spriteBounds, float scale, float spacing)
		{
			Vector3 offset = new Vector3();
			offset.x = -(spriteBounds.center.x + spriteBounds.extents.x) * scale;
			switch (vertical)
			{
				case VerticalPivot.Top:
					offset.y = -(spriteBounds.center.y + spriteBounds.extents.y) * scale;
					break;
				case VerticalPivot.Bottom:
					offset.y = spriteBounds.center.y + spriteBounds.extents.y * scale;
					break;
			}

			trans.localPosition = caret + offset;
			trans.localScale = new Vector3(scale, scale, 1f);

			caret.x -= spriteBounds.size.x * scale + spacing;
		}

		#endregion

		#region Material util

		protected static Material GetMaterialForSprite(Material mat)
		{
			if (mat == null)
				return DefaultSpriteMaterialUtil.DefaultSpriteMaterial;
			return mat;
		}

		#endregion

	}

}
