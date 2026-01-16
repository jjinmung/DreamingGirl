using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Inven : UI_Scene
{
    enum GameObjects
    {
        GridPanel
    }

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();

        Bind<GameObject>(typeof(GameObjects));

        GameObject gridPanel = Get<GameObject>((int)GameObjects.GridPanel);
        foreach (Transform child in gridPanel.transform)
            Managers.Resource.Destroy(child.gameObject);

 
    }
}
