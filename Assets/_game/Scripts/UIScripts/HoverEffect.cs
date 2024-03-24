using UnityEngine;
using UnityEngine.EventSystems;
namespace _game.Scripts.UIScripts
{
    public class HoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private GameObject _hover;
        public void OnPointerEnter(PointerEventData eventData) { _hover.SetActive(true); }
        public void OnPointerExit(PointerEventData eventData) { _hover.SetActive(false); }
    }
}
