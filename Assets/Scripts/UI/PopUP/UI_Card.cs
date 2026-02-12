using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_Card : UI_Base
{
    private AbilityID[] abilityIDs;
    private List<UI_Card> uiCards;

    enum GameObjects
    {
        Star1_Image,
        Star2_Image,
        Star3_Image,
        Star4_Image,
        Star5_Image,
        EmptyStar1_Image,
        EmptyStar2_Image,
        EmptyStar3_Image,
        EmptyStar4_Image,
        EmptyStar5_Image,
    }
    enum Images
    {
        Card_Icon,
    }

    enum Texts
    {
        NameText,
        DescriptionText,
    }



    public override void Init()
    {
		Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));
	}
    
    public async UniTask SetCard(AbilityID ID)
    {
        AbilityInstance abilityInstance=Managers.Data.AbilityDict[ID];
        if (abilityInstance != null)
        {
            GetImage((int)Images.Card_Icon).sprite =
                await Managers.Resource.LoadIconAsync(abilityInstance.data.iconRef);
            if(GetText((int)Texts.DescriptionText)!=null)
                GetText((int)Texts.DescriptionText).text = abilityInstance.data.description[abilityInstance.stack];
            if (GetText((int)Texts.NameText) != null)
                GetText((int)Texts.NameText).text = abilityInstance.data.abilityName;
            var stack = abilityInstance.stack;
            for (int i = 0; i < stack; i++)
            {
                GetObject(i).SetActive(true);
                GetObject(i+5).SetActive(false);
            }

            for (int i = stack; i < 5; i++)
            {
                GetObject(i).SetActive(false);
                GetObject(i+5).SetActive(true);
            }
        }
    }
    
    

   

   

}
