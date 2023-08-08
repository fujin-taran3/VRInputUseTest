// --------------------------------------------------------- 
// Mouse_BowManager.cs 
// 
// CreateDay: 
// Creator  : 
// --------------------------------------------------------- 
using UnityEngine;
[RequireComponent(typeof(BowTransformControl))]
public class Mouse_BowManager : CanDraw_BowManager
{
    #region variable

    new IFBowTransformControl_Mouse _transformControl = default;

    const int LEFT_MOUSE_BUTTON = 0;

    #endregion

    #region property
    #endregion
    #region method

    protected override void Start()
    {

        base.Start();

        _transformControl = base._transformControl as IFBowTransformControl_Mouse;
    }

    private void Update()
    {
        BowUpdateCallProcess();
    }

    protected override void ProcessOfGrapObject()
    {
        base.ProcessOfGrapObject();
    }

    protected override void ProcessOfHoldObject()
    {
        BowStringHold();
    }

    protected override void ProcessOfReleaseObjcect()
    {
        BowShotStart();
    }
   

    protected override void SetInputDelegate()
    {
        // 左クリック押した時
        _grapTriggerInput = () => Input.GetMouseButton(LEFT_MOUSE_BUTTON);
 
        // 左クリック離した時
        _releaseTriggerInput = () => !Input.GetMouseButton(LEFT_MOUSE_BUTTON);

        // マウスの位置を取得するデリゲードをセット
        _transformControl.SetMousePositionDelegate();
    }

    protected override float GetShotPercentPower()
    {
        return _transformControl.GetPercentDrawDistance(drawLimitDistance, drawDistancePercentMaxPower);
    }

    protected override void AddBowShotProcess()
    {
        return;// 追加なし
    }

    protected override void CastInterface()
    {
        _transformControl = base._transformControl as IFBowTransformControl_Mouse;
    }

    #endregion
}