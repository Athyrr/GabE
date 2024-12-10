using GabE.Module.ECS;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class UI_Prosperity : MonoBehaviour
{
    [SerializeField]
    Slider _slider = null;

    EntityManager _manager;

    //void Start()
    //{
    //    _manager = World.DefaultGameObjectInjectionWorld.EntityManager;
    //    _manager.CreateEntityQuery<ECS_GlobalLifecyleFragment>();

    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    _slider.value = _manager.Get
    //}
}
