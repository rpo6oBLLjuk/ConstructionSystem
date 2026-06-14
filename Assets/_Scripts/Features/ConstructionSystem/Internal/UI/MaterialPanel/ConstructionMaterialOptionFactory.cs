using System;
using System.Collections.Generic;
using UnityEngine;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class ConstructionMaterialOptionFactory : MonoBehaviour
    {
        [SerializeField] private ConstructionMaterialOptionView _prefab;
        [SerializeField] private Transform _root;
        private List<GameObject> _pool = new();


        private void Start() => _prefab.gameObject.SetActive(false);

        public ConstructionMaterialOptionView Create(RoomMaterialData data, Action selectHandler)
        {
            ConstructionMaterialOptionView view = Instantiate(_prefab.gameObject, _root).GetComponent<ConstructionMaterialOptionView>();
            view.gameObject.SetActive(true);
            _pool.Add(view.gameObject);

            view.Initialize(data, selectHandler);

            return view;
        }
        public void Clear()
        {
            _pool.ForEach(view => Destroy(view));
            _pool.Clear();
        }
    }
}
