using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using R3;
using System;

namespace radiants.SpriteDigits
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(RectTransform))]
	public class SpriteDigitsFloat : SpriteDigitsBase
	{
		#region Serialize/Observables

		[SerializeField]
		private SerializableReactiveProperty<int> _DisplayDecimalPlaces = new SerializableReactiveProperty<int>(2);
		public int DisplayDecimalPlaces
		{
			get { return _DisplayDecimalPlaces.Value; }
			set { _DisplayDecimalPlaces.Value = value; }
		}

		[SerializeField]
		private SerializableReactiveProperty<double> _Value = new SerializableReactiveProperty<double>(0d);
		public double Value
		{
			get { return _Value.Value; }
			set { _Value.Value = value; }
		}

		#endregion

		#region Subscribes

		protected override void SubscribeObservables()
		{
			_Value.Subscribe(_ => ApplyNumbers()).AddTo(Disposables);
			_DisplayDecimalPlaces.Subscribe(_ => ApplyNumbers()).AddTo(Disposables);
		}

		#endregion


		#region Child Sprites Management

		private List<DigitsDisplayContainer> NumberDisplays = new List<DigitsDisplayContainer>();

		private DigitsDisplayContainer MinusDisplay = null;

		private DigitsDisplayContainer DecimalPointDisplay = null;

		protected override void PrepareDisplays()
		{
			if (MinusDisplay == null)
				MinusDisplay = CreateChildDisplay();

			if (DecimalPointDisplay == null)
				DecimalPointDisplay = CreateChildDisplay();

			//minimum required
			PrepareNumberDisplays(DisplayDecimalPlaces + 1);
		}

		private void PrepareNumberDisplays(int requiredNumber)
		{
			if (NumberDisplays.Count >= requiredNumber) return;

			for (int i = NumberDisplays.Count; i < requiredNumber; i++)
			{
				NumberDisplays.Add(CreateChildDisplay());
			}
		}
		protected override void UnLinkAllDisplays()
		{
			NumberDisplays.Clear();
			MinusDisplay = null;
			DecimalPointDisplay = null;
		}


		protected override void ActForAllDisplays(Action<DigitsDisplayContainer> action)
		{
			foreach (var number in NumberDisplays)
			{
				action?.Invoke(number);
			}
			action?.Invoke(MinusDisplay);
			action?.Invoke(DecimalPointDisplay);
		}

		#endregion





		#region Apply Numbers

		protected override void ApplyNumbers()
		{
			if (Digits == null) return;
			if (!Digits.CheckNumbersFull()) return;

			//to avoid float accuracy issue, convert value to decimal
			if(Value > (double)decimal.MaxValue)
			{
				Debug.LogWarning("Value is bigger than decimal.MaxValue. SpriteDigits cannot display it.");
				Value = (double)decimal.MaxValue;
			}
			decimal num = (decimal)Value;
			bool displayMinus = false;

			if (num < 0)
			{
				num = -num;
				displayMinus = true;
			}

			int digitsBeforePoint = CalcDigitsBeforeDecimalPoint(num);

			//prepare renderers dynamically
			PrepareNumberDisplays(DisplayDecimalPlaces + digitsBeforePoint);

			//check sprite height
			float spriteHeight = Digits.NumberSprites[0].bounds.size.y;
			float size = Mathf.Min(Size, MyRectTransform.rect.height);
			float letterScale = size / spriteHeight;

			//set sprite and check sprites' total width
			float originalWidth = SetNumberSpriteToDisplays(num, ref digitsBeforePoint, displayMinus);
			float widthWithSpace = originalWidth * letterScale + (digitsBeforePoint + DisplayDecimalPlaces) * Spacing;
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
			SetupSpritePositions(letterScale, spacingScale, widthWithSpace, digitsBeforePoint, displayMinus);
		}


		private float SetNumberSpriteToDisplays(decimal num, ref int digitsBeforePoint, bool displayMinus)
		{
			//calc sprites' total width
			float width = 0;

			bool carryUp = false;

			//after point
			for (int i = 0; i < DisplayDecimalPlaces; i++)
			{
				int place = DisplayDecimalPlaces - i;
				int number = i == 0 ? RoundNumberOfPlaceAfterPoint(num, place) : GetNumberOfPlaceAfterPoint(num, place);

				//round carry-up
				if (carryUp) ++number;
				if (number > 9)
				{
					number -= 10;
					carryUp = true;
				}
				else
				{
					carryUp = false;
				}

				var sprite = Digits.NumberSprites[number];
				NumberDisplays[i].enabled = true;
				NumberDisplays[i].sprite = sprite;

				width += sprite.bounds.size.x;
			}

			//point
			DecimalPointDisplay.enabled = true;
			DecimalPointDisplay.sprite = Digits.DecimalPointSprite;
			width += Digits.DecimalPointSprite.bounds.size.x;

			//before point
			for (int i = 0; i < digitsBeforePoint; i++)
			{
				int number;
				if (DisplayDecimalPlaces == 0)
					number = RoundNumberOfPlaceBeforePoint(num, i);
				else
					number = GetNumberOfPlaceBeforePoint(num, i);

				//round carry-up
				if (carryUp) ++number;
				if (number > 9)
				{
					number -= 10;
					carryUp = true;
				}
				else
				{
					carryUp = false;
				}


				var sprite = Digits.NumberSprites[number];

				int rendererIndex = i + DisplayDecimalPlaces;
				NumberDisplays[rendererIndex].enabled = true;
				NumberDisplays[rendererIndex].sprite = sprite;

				width += sprite.bounds.size.x;
			}

			//carry final digit
			if(carryUp)
			{
				int finalIndex = DisplayDecimalPlaces + digitsBeforePoint;
				PrepareNumberDisplays(finalIndex+1);
				NumberDisplays[finalIndex].enabled = true;
				NumberDisplays[finalIndex].sprite = Digits.NumberSprites[1];
				width += Digits.NumberSprites[1].bounds.size.x;

				++digitsBeforePoint;
			}


			//disabled
			for (int i = DisplayDecimalPlaces + digitsBeforePoint; i < NumberDisplays.Count; i++)
			{
				NumberDisplays[i].enabled = false;
			}


			//minus
			MinusDisplay.enabled = displayMinus;
			if(displayMinus)
			{
				MinusDisplay.sprite = Digits.MinusDisplaySprite;
				width += Digits.MinusDisplaySprite.bounds.size.x;
			}

			return width;
		}

		private void SetupSpritePositions(float letterScale, float spacingScale, float scaledWidth, int digitsBeforePoint, bool displayMinus)
		{
			Vector3 pivotOrigin = GetPivotOrigin(HorizontalPivot, VerticalPivot, MyRectTransform.rect, scaledWidth);
			Vector3 caret = pivotOrigin;

			//after point
			for (int i = 0; i < DisplayDecimalPlaces; i++)
			{
				var renderer = NumberDisplays[i];
				var spriteBounds = renderer.sprite.bounds;
				SetDisplayPosition(ref caret, renderer.transform, HorizontalPivot, VerticalPivot, spriteBounds, letterScale, Spacing * spacingScale);
			}

			//point
			SetDisplayPosition(ref caret, DecimalPointDisplay.transform, HorizontalPivot, VerticalPivot, DecimalPointDisplay.sprite.bounds,
				letterScale, Spacing * spacingScale);

			//before point
			for (int i = 0; i < digitsBeforePoint; i++)
			{
				int index = i + DisplayDecimalPlaces;
				var renderer = NumberDisplays[index];
				var spriteBounds = renderer.sprite.bounds;
				SetDisplayPosition(ref caret, renderer.transform, HorizontalPivot, VerticalPivot, spriteBounds, letterScale, Spacing * spacingScale);
			}

			//set minus renderer if display
			if (displayMinus)
			{
				SetDisplayPosition(ref caret, MinusDisplay.transform, HorizontalPivot, VerticalPivot, MinusDisplay.sprite.bounds,
					letterScale, Spacing * spacingScale);
			}
		}

		#endregion

		#region Math

		private static int CalcDigitsBeforeDecimalPoint(decimal num)
		{
			int ret = 1;
			while(num >= 10)
			{
				++ret;
				num /= 10;
			}

			return ret;
		}

		private static int RoundNumberOfPlaceAfterPoint(decimal num, int place)
		{
			decimal n = num;
			n *= Power10(place - 1);
			n -= Math.Floor(n);
			return (int)Math.Round(n * 10);

		}
		private static int GetNumberOfPlaceAfterPoint(decimal num, int place)
		{
			decimal n = num;
			n *= Power10(place - 1);
			n -= Math.Floor(n);
			return (int)Math.Floor(n * 10);
		}
		private static int RoundNumberOfPlaceBeforePoint(decimal num, int digit)
		{
			decimal n = num;
			n /= Power10(digit + 1);
			n -= Math.Floor(n);
			return (int)Math.Round(n * 10);
		}
		private static int GetNumberOfPlaceBeforePoint(decimal num, int digit)
		{
			decimal n = num;
			n /= Power10(digit + 1);
			n -= Math.Floor(n);
			return (int)Math.Floor(n * 10);
		}

		private static decimal Power10(int _power)
		{
			if (_power < 0) return 1;
			decimal ret = 1;
			for (int i = 0; i < _power; i++)
			{
				ret *= 10;
			}
			return ret;
		}

		#endregion

	}
}