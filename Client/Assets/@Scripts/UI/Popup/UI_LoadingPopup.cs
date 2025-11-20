using System;
using UnityEngine;


public class UI_LoadingPopup : UI_Popup
{
    enum GameObjects
    {
    }

    enum Sliders
    {
        LoadingSlider,
    }

    public Action OnClosed;
    
    protected override void Awake()
    {
        base.Awake();

        BindSliders(typeof(Sliders));
    }

    public void SetSliderValue(float sliderValue)
    {
        //Debug.Log(sliderValue);
        GetSlider((int)Sliders.LoadingSlider).value = sliderValue;
    }

}
