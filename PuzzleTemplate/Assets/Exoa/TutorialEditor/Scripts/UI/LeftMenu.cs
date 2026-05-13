using System.Collections.Generic;
using UnityEngine;

namespace Exoa.TutorialEngine
{
    public class LeftMenu : TutorialEditorGenericMenu
    {
        public static LeftMenu instance;

        public GameObject leftMenuItemPrefab;
        public float itemOffset = 2;
        public RectTransform levelItemsContainerRect;
        private List<LevelLeftMenuItem> levelLeftMenuItemList;

        override public void Start()
        {
            instance = this;
            base.Start();
        }

        override public void BuildMenu()
        {
            TextAsset[] assets = Resources.LoadAll<TextAsset>("Tutorials");


            levelItemsContainerRect.ClearChildren();
            levelLeftMenuItemList = new List<LevelLeftMenuItem>();


            float itemHeight = 0;
            foreach (TextAsset ta in assets)
            {
                string path = ta.name;

                GameObject inst = Instantiate(leftMenuItemPrefab);
                inst.transform.SetParent(levelItemsContainerRect);
                RectTransform r = inst.GetComponent<RectTransform>();
                r.localScale = Vector3.one;
                itemHeight = r.sizeDelta.y;
                LevelLeftMenuItem plmi = r.GetComponent<LevelLeftMenuItem>();
                plmi.SetLevelPath(path);

                levelLeftMenuItemList.Add(plmi);
            }

            print(" leftMenu size " + levelLeftMenuItemList.Count + " " + (levelLeftMenuItemList.Count * (itemHeight + itemOffset)));
            //levelItemsContainerRect.rect.
            levelItemsContainerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, levelLeftMenuItemList.Count * (itemHeight + itemOffset));

        }

    }
}
