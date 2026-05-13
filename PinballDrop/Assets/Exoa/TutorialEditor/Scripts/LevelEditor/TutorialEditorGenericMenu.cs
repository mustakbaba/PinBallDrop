using UnityEngine;
using UnityEngine.UI;

namespace Exoa.TutorialEngine
{
    public class TutorialEditorGenericMenu : MonoBehaviour
    {

        public Button bgButton;
        public Button openButton;
        private bool isOpen;
        private RectTransform rt;
        public float closedX = -240f;
        public float openX = 240f;

        private void OnDestroy()
        {
            TutorialEvents.OnTutorialLoaded -= OnTutorialLoeaded;
        }
        public virtual void Start()
        {
            rt = GetComponent<RectTransform>();
            Vector3 anchoredPosition = rt.anchoredPosition;
            anchoredPosition.x = closedX;
            rt.anchoredPosition = anchoredPosition;
            openButton.onClick.AddListener(OnClickOpen);
            bgButton.onClick.AddListener(OnClickOpen);
            bgButton.gameObject.SetActive(false);


            BuildMenu();
            TutorialEvents.OnTutorialLoaded += OnTutorialLoeaded;

        }

        private void OnTutorialLoeaded()
        {
            BuildMenu();
        }

        public virtual void BuildMenu()
        {

        }

        private void OnClickOpen()
        {
            isOpen = !isOpen;
            CloseMenu(!isOpen);


        }
        public void CloseMenu(bool close = true)
        {
            rt.anchoredPosition = new Vector2(close ? closedX : openX, rt.anchoredPosition.y);
            isOpen = !close;
            bgButton.gameObject.SetActive(true);
            bgButton.GetComponent<Image>().color = new Color(0, 0, 0, close ? 0 : .4f);
            bgButton.gameObject.SetActive(isOpen);

        }
    }
}
