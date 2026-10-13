using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GuardianesAmazonicos.Nivel1.HU02
{
    [RequireComponent(typeof(Button))]
    public sealed class AccessibleButton : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        private Outline _focus;
        private Outline _halo;

        private void Awake()
        {
            var graphic = GetComponent<Button>().targetGraphic.gameObject;
            _halo = graphic.AddComponent<Outline>();
            _halo.effectColor = Color.white;
            _halo.effectDistance = new Vector2(6, -6);
            _focus = graphic.AddComponent<Outline>();
            _focus.effectColor = new Color32(9, 39, 30, 255);
            _focus.effectDistance = new Vector2(3, -3);
            _halo.enabled = false;
            _focus.enabled = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            _focus.enabled = _halo.enabled = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _focus.enabled = _halo.enabled = false;
        }
    }
}
