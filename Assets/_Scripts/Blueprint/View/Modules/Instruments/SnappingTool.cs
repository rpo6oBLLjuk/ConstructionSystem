using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SnappingTool : ButtonSpriteSwapper
{
    [Inject] BlueprintManager _blueprintManager;

    private void Start() => SetActive(false);

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift))
            SetActive(true);
        else if (Input.GetKeyUp(KeyCode.LeftShift))
            SetActive(false);
    }

    public override void SetActive(bool isActive)
    {
        base.SetActive(isActive);

        if (IsActive)
            _blueprintManager.PointsController.EnableSnapping();
        else
            _blueprintManager.PointsController.DisableSnapping();
    }
}
