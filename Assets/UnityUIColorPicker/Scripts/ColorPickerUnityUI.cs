using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UnityUIColorPicker.Scripts
{
    public class ColorPickerUnityUI : MonoBehaviour
    {
        [Tooltip("Is the image a circle")] public bool circular;
        [Tooltip("Picture to use")] public Image colorPalette;
        [Tooltip("Thumb to use")] public Image thumb;
        [Tooltip("Output Color.")] public Color value;

        [FormerlySerializedAs("WasClicked")] [HideInInspector]
        public bool wasClicked = false;

        public delegate void ChangeAction(Color color);

        public static event ChangeAction OnChange;

        private Vector2 _spectrumXY; // the size of the palette

        /// <summary>
        /// Tis is to make the code more clean down below. We store the values most often used.
        /// </summary>
        private Bounds _pictureBounds; // the bounds of the palette

        private Vector3 _max; // max bounds
        private Vector3 _min; // min bounds

        private CanvasScaler _myScale;
        //////////////////	

        private void Start()
        {
            _myScale = colorPalette.canvas.transform.GetComponent<CanvasScaler>();


            _spectrumXY = new Vector2(colorPalette.GetComponent<RectTransform>().rect.width * _myScale.transform.localScale.x, colorPalette.GetComponent<RectTransform>().rect.height * _myScale.transform.localScale.y);
            _pictureBounds = colorPalette.GetComponent<Collider2D>().bounds;
            _max = _pictureBounds.max;
            _min = _pictureBounds.min;
        }

        public static Vector3 MultiplyVectors(Vector3 V1, Vector3 V2)
        {
            float[] x = {V1.x, V2.x};
            float[] y = {V1.y, V2.y};
            float[] z = {V1.z, V2.z};
            return new Vector3(x[0] * x[1], y[0] * y[1], z[0] * z[1]);
        }

        public void ResetTumb()
        {
            thumb.GetComponent<RectTransform>().localPosition = Vector3.zero;
        }

        // called by event on object
        public void OnPress()
        {
            UpdateThumbPosition();
            wasClicked = true;
        }

        // called by event on object
        public void OnDrag()
        {
            UpdateThumbPosition();
            wasClicked = true;
        }


        //get color of mouse point
        private Color GetColor()
        {
            Vector2 spectrumScreenPosition = colorPalette.transform.position;
            Vector2 thumbScreenPosition = thumb.transform.position;
            var position = thumbScreenPosition - spectrumScreenPosition + _spectrumXY * 0.5f;
            var texture = colorPalette.mainTexture as Texture2D;
            if (circular)
            {
                _myScale = colorPalette.canvas.transform.GetComponent<CanvasScaler>();


                _spectrumXY = new Vector2(colorPalette.GetComponent<RectTransform>().rect.width * _myScale.transform.localScale.x, colorPalette.GetComponent<RectTransform>().rect.height * _myScale.transform.localScale.y);
                _pictureBounds = colorPalette.GetComponent<Collider2D>().bounds;
                _max = _pictureBounds.max;
                _min = _pictureBounds.min;

                position = new Vector2((position.x / (colorPalette.GetComponent<RectTransform>().rect.width)),
                    (position.y / (colorPalette.GetComponent<RectTransform>().rect.height)));
                var circularSelectedColor = texture.GetPixelBilinear(position.x / _myScale.transform.localScale.x, position.y / _myScale.transform.localScale.y);
                circularSelectedColor.a = 1;
                return circularSelectedColor;
            }
            else
            {
                position = new Vector2((position.x / colorPalette.GetComponent<RectTransform>().rect.width), (position.y / colorPalette.GetComponent<RectTransform>().rect.height));
            }

            var selectedColor = texture.GetPixelBilinear(position.x / _myScale.transform.localScale.x, position.y / _myScale.transform.localScale.y);
            selectedColor.a = 1;
            return selectedColor;
        }

        //move the object only where the picture is
        private void UpdateThumbPosition()
        {
            if (circular && colorPalette.GetComponent<CircleCollider2D>())
            {
                var center = transform.position;
                var position = Input.mousePosition;
                var offset = position - center;
                var set = Vector3.ClampMagnitude(offset, (colorPalette.GetComponent<CircleCollider2D>().radius * _myScale.transform.localScale.x));
                var newPos = center + set;
                if (thumb.transform.position == newPos) return;
                thumb.transform.position = newPos;
                value = GetColor();
            }
            else
            {
                if (circular)
                {
                    Debug.LogError("No 'CircleCollider2D' found on object. Please add a CircleCollider or turn off 'circular'.");
                }

                _spectrumXY = new Vector2(colorPalette.GetComponent<RectTransform>().rect.width * _myScale.transform.localScale.x, colorPalette.GetComponent<RectTransform>().rect.height * _myScale.transform.localScale.y);
                _pictureBounds = colorPalette.GetComponent<Collider2D>().bounds;
                _max = _pictureBounds.max;
                _min = _pictureBounds.min;

                var x = Mathf.Clamp(Input.mousePosition.x, _min.x, _max.x + 1);
                var y = Mathf.Clamp(Input.mousePosition.y, _min.y, _max.y);
                var newPos = new Vector3(x, y, transform.position.z);
                if (thumb.transform.position == newPos) return;
                thumb.transform.position = newPos;
                value = GetColor();
            }

            OnChange?.Invoke(value);
        }
    }
}