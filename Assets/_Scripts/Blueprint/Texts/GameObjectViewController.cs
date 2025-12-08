using System.Collections.Generic;
using UnityEngine;

public class GameObjectViewController : IViewController
{
    public IReadOnlyList<GameObject> Texts => _texts;
    public bool IsActive
    {
        get => _isActive;
        private set
        {
            _isActive = value;
            Q(_isActive);
        }
    }

    private List<GameObject> _texts;
    private bool _isActive;


    public void SetActive(bool isActive) => _isActive = isActive;

    public void AddGameObject(GameObject textContaier)
    {
        _texts.Add(textContaier);
        textContaier.SetActive(_isActive);
    }
    public void RemoveGameObject(GameObject textContainer) => _texts.Remove(textContainer);

    private void Q(bool isActive) => _texts.ForEach(text => text.SetActive(isActive));
}

public interface IViewController
{
    public bool IsActive {  get; }
    public void SetActive(bool isActive);

    public void AddGameObject(GameObject textContaier);
    public void RemoveGameObject(GameObject textContaier);
}