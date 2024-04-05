using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using R3;
using System;

namespace radiants.SpriteDigits
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(RectTransform))]
	public class SpriteDigits : SpriteDigitsBase
	{
		#region Serialize/Observables

		[SerializeField]
		private SerializableReactiveProperty<long> _Value = new SerializableReactiveProperty<long>(0);
		public long Value
		{
			get { return _Value.Value; }
			set { _Value.Value = value; }
		}


		[SerializeField]
		private SerializableReactiveProperty<int> _MaxDigitNum = new SerializableReactiveProperty<int>(-1);
		public int MaxDigitNum
		{
			get { return _MaxDigitNum.Value; }
			set { _MaxDigitNum.Value = value; }
		}

		[SerializeField]
		private SerializableReactiveProperty<PaddingMode> _PaddingMode = new SerializableReactiveProperty<PaddingMode>(PaddingMode.Pad);
		public PaddingMode PaddingMode
		{
			get { return _PaddingMode.Value; }
			set { _PaddingMode.Value = value; }
		}

		#endregion


		#region Subscribes

		protected override void SubscribeObservables()
		{
			_MaxDigitNum.Subscribe(_num => { PrepareNumberRenderers(_num); ApplyNumbers(); }).AddTo(Disposables);

			_Value.Subscribe(_ => ApplyNumbers()).AddTo(Disposables);
			_PaddingMode.Subscribe(_ => ApplyNumbers()).AddTo(Disposables);
		}

		#endregion


		#region Child Sprites Management

		private List<DigitsDisplayContainer> NumberDisplays = new List<DigitsDisplayContainer>();

		private DigitsDisplayContainer MinusDisplay = null;

		protected override void PrepareDisplays()
		{
			if (MinusDisplay == null)
				MinusDisplay = CreateChildDisplay();

			PrepareNumberRenderers(MaxDigitNum);
		}

		private void PrepareNumberRenderers(int digitNum)
		{
			if (NumberDisplays.Count >= digitNum) return;

			for (int i = NumberDisplays.Count; i < digitNum; i++)
			{
				NumberDisplays.Add(CreateChildDisplay());
			}
		}

		protected override void UnLinkAllDisplays()
		{
			NumberDisplays.Clear();
			MinusDisplay = null;
		}

		protected override void ActForAllDisplays(Action<DigitsDisplayContainer> action)
		{
			foreach (var number in NumberDisplays)
			{
				action?.Invoke(number);
			}
			action?.Invoke(MinusDisplay);
		}

		#endregion


		#region Apply Numbers

		protected override void ApplyNumbers()
		{
			if (Digits == null) return;
			if (!Digits.CheckNumbers()) return;

			long num = Value;
			bool displayMinus = false;

			if(num < 0)
			{
				//ignores minus value if minus sprite not found
				if (Digits.MinusDisplaySprite == null)
					num = 0;
				else
				{
					num = -num;
					displayMinus = true;
				}
			}

			int digitNum = GetDigitNumber(num);

			//counter-stop
			if(MaxDigitNum != -1 && MaxDigitNum < digitNum)
			{
				num = Power(10, MaxDigitNum) - 1;
				digitNum = MaxDigitNum;
			}

			int displayDigitNum = digitNum;
			if (MaxDigitNum == -1)
			{
				//unlimited digits
				PrepareNumberRenderers(digitNum);
			}
			else if (PaddingMode == PaddingMode.ZeroFill)
			{
				//zero fill
				displayDigitNum = MaxDigitNum;
			}

			//check sprite height
			float spriteHeight = Digits.NumberSprites[0].bounds.size.y;
			float size = Mathf.Min(Size, MyRectTransform.rect.height);
			float letterScale = size / spriteHeight;

			//set sprite and check sprites' total width
			float originalWidth = SetNumberSpriteToDisplays(num, displayDigitNum, displayMinus);
			float widthWithSpace = originalWidth * letterScale + (displayDigitNum - 1) * Spacing;
			if (displayMinus) widthWithSpace += Spacing;

			//determine scale
			float spacingScale = 1f;
			if (MyRectTransform.rect.width < widthWithSpace)
			{
				spacingScale = MyRectTransform.rect.width / widthWithSpace;
				letterScale = letterScale * spacingScale;
				widthWithSpace = MyRectTransform.rect.width;
			}

			//set position
			SetupSpritePositions(letterScale, spacingScale, widthWithSpace, displayDigitNum, displayMinus);
		}

		private float SetNumberSpriteToDisplays(long num, int displayDigitNum, bool displayMinus)
		{
			//calc sprites' total width
			float width = 0;

			for (int i = 0; i < NumberDisplays.Count; i++)
			{
				var disp = NumberDisplays[i];
				if(i >= displayDigitNum)
				{
					disp.enabled = false;
					continue;
				}

				disp.enabled = true;
				int count = (int)(num % 10);

				var sprite = Digits.NumberSprites[count];
				var spriteOriginalBounds = sprite.bounds.size;
				disp.sprite = sprite;

				num /= 10;
				width += spriteOriginalBounds.x;
			}

			MinusDisplay.enabled = displayMinus;
			if (displayMinus)
			{
				var minusSprite = Digits.MinusDisplaySprite;
				MinusDisplay.sprite = minusSprite;
				if (minusSprite != null)
					width += minusSprite.bounds.size.x;
			}

			return width;
		}

		private void SetupSpritePositions(float letterScale, float spacingScale, float scaledWidth, int displayDigitNum, bool displayMinus)
		{
			Vector3 pivotOrigin = GetPivotOrigin(HorizontalPivot, VerticalPivot, MyRectTransform.rect, scaledWidth);
			Vector3 caret = pivotOrigin;

			for (int i = 0; i < displayDigitNum; ++i)
			{
				var disp = NumberDisplays[i];
				var spriteBounds = disp.sprite.bounds;

				//spriteBounds.min
				SetDisplayPosition(ref caret, disp.transform, HorizontalPivot, VerticalPivot, spriteBounds, letterScale, Spacing * spacingScale);
			}

			//set minus renderer if display
			if(displayMinus)
			{
				SetDisplayPosition(ref caret, MinusDisplay.transform, HorizontalPivot, VerticalPivot, MinusDisplay.sprite.bounds,
					letterScale, Spacing * spacingScale);
			}
		}

		#endregion

		#region Math

		private static int GetDigitNumber(long num)
		{
			int digitNum = 0;

			while (num != 0)
			{
				++digitNum;
				num /= 10;
			}

			if (digitNum == 0) digitNum = 1;
			return digitNum;
		}

		private static long Power(int _base, int _power)
		{
			if (_power < 0) return 0;
			long ret = 1;
			for (int i = 0; i < _power; i++)
			{
				ret *= _base;
			}
			return ret;
		}

		#endregion
	}
}