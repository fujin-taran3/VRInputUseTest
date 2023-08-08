// --------------------------------------------------------- 
// BowTransformControl.cs 
// 
// CreateDay: 
// Creator  : 
// --------------------------------------------------------- 
using UnityEngine;

public interface IFBowTransformControl
{
    /// <summary>
    /// 引くオブジェクトのトランスフォームはこれ経由で取得
    /// </summary>
    Transform GetDrawObjectTransform { get; }

    /// <summary>
    /// 弓の向いている方向を取得(world)
    /// </summary>
    /// <returns></returns>
    Vector3 GetBowFoward();

    float GetDrawDistance();

    float GetPercentDrawDistance(float drawLimitDistance, float drawDistancePercentMaxPower);

    void GrapSetTransform(Transform arrow);

    void HoldingSetTransform();

    void AfterShotResetTransform();

    void ResetDrawObject();

    Quaternion GetShotingRotetion();

    Vector3 GetHandPosition { get; }

}

interface IFBowTransformControl_Bow : IFBowTransformControl
{
    void SetBowTransformInHand(InputManagement.EmptyHand emptyHand);

    void SetEmptyHandPositionDelegete(InputManagement.EmptyHand emptyHand);

   
}

interface IFBowTransformControl_Mouse : IFBowTransformControl
{
    void SetMousePositionDelegate();

}

interface IFBowTransformControl_FPS
{
    void SetArrowFirstTransform(Transform arrow, Transform parent);

    void WASDMove(Vector3 foward, float moveSpeed);
}

public class BowTransformControl : MonoBehaviour, IFBowTransformControl_Bow, IFBowTransformControl_Mouse, IFBowTransformControl_FPS, IFBowTransformControl
{
    #region かつてpublicだった変数

    [SerializeField] Transform _handLeftPosition = default;

    [SerializeField] Transform _handRightPosition = default;
    /// <summary>
    /// 引くオブジェクト
    /// </summary>
    [SerializeField] Transform _drawObject = default;

    [SerializeField] Transform _changeHandObjectTransform = default;

    [SerializeField] Transform _selectUI;

    [SerializeField] Transform _selectUILeftPosition = default;

    [SerializeField] Transform _selectUIRightPosition = default;

    #endregion

    #region クラス、構造体

    delegate Vector3 HandPositionDelegate();


    /// <summary>
    /// 手の状態管理型
    /// </summary>
    enum HandStats
    {
        None,
        Hold,
    }

    HandPositionDelegate _handPositionDelegate = default;

    Transform _transform = default;

    Vector3 _firstDrawObjectPositon = default;

    Quaternion _myQuaternion = default;

    float _distanceCameraToDrawObject = default;

    #endregion

    private void Awake()
    {
        #region 初期化たち

        _transform = transform;

        if(_drawObject is null)
        {
            _drawObject = this.transform;
        }

        _firstDrawObjectPositon = _drawObject.localPosition;

        _myQuaternion = _transform.localRotation;

        #endregion

    }

    #region public関数

    /// <summary>
    /// 現在の弓を引いた距離が返される
    /// </summary>
    public float GetDrawDistance()
    {
        return Vector3.Distance(_firstDrawObjectPositon, _drawObject.localPosition);
    }

    /// <summary>
    /// 現在の弓の引いた距離(%)
    /// </summary>
    /// <param name="drawLimitDistance">弓の弦の引ける限界距離</param>
    /// <param name="drawDistancePercentMaxPower">矢の威力が最高になる距離(%)</param>
    public float GetPercentDrawDistance(float drawLimitDistance, float drawDistancePercentMaxPower)
    {
        return Vector3.Magnitude(_drawObject.position - _transform.position) / (drawLimitDistance * drawDistancePercentMaxPower);
    }

    public Transform GetDrawObjectTransform => _drawObject.transform;

    public Vector3 GetHandPosition => _handPositionDelegate();

    /// <summary>
    /// 弓の前方のワールド方向ベクトルを取得
    /// </summary>
    public Vector3 GetBowFoward()
    {
        return _transform.forward - _transform.position;
    }

    /// <summary>
    /// 掴むときのトランスフォーム変化
    /// </summary>
    public void GrapSetTransform(Transform arrow)
    {
        arrow.transform.rotation = _transform.rotation;

        arrow.transform.parent = _drawObject.transform;

        arrow.transform.position -= arrow.transform.GetChild(0).position - arrow.transform.position;

        _drawObject.position = _handPositionDelegate();

    }

    /// <summary>
    /// 掴み中のトランスフォーム変化
    /// </summary>
    public void HoldingSetTransform()
    {
        _drawObject.position = _handPositionDelegate();

        // 弓と弦の方向ベクトルで弓の角度決める、Zは可変
        TurnBow();


        void TurnBow()
        {

            float angleBowZ = _transform.rotation.eulerAngles.z;

            _transform.rotation = Quaternion.LookRotation(_transform.position - _drawObject.transform.position);

            _transform.rotation = Quaternion.Euler(_transform.rotation.eulerAngles.x,
                _transform.rotation.eulerAngles.y, angleBowZ);

        }
    }

    /// <summary>
    /// 弓を打った後のトランスフォームリセット
    /// </summary>
    public void AfterShotResetTransform()
    {

        ResetDrawObject();

        _transform.localRotation = _myQuaternion;

    }

    /// <summary>
    /// 弦の位置を元にもどす
    /// </summary>
    public void ResetDrawObject()
    {
        _drawObject.transform.localPosition = _firstDrawObjectPositon;
    }

    /// <summary>
    /// 指定した手に応じて弓のトランスフォームをセットする
    /// </summary>
    /// <param name="emptyHand">空の手</param>
    public void SetBowTransformInHand(InputManagement.EmptyHand emptyHand)
    {
        // 左手が空いていたら
        if (emptyHand == InputManagement.EmptyHand.Left)
        {
            _transform.parent = _handRightPosition;
            _selectUI.parent = _selectUIRightPosition;
            
        }
        // 右手が空いていたら
        else
        {
            _transform.parent = _handLeftPosition;
            _selectUI.parent = _selectUILeftPosition;
        }
        
        _transform.localPosition = Vector3.zero;

        _transform.localRotation = _myQuaternion;

        _selectUI.localPosition = Vector3.zero;

        _selectUI.localRotation = Quaternion.identity;

    }

    /// <summary>
    /// 手のインプットの設定
    /// </summary>
    public void SetEmptyHandPositionDelegete(InputManagement.EmptyHand emptyHand)
    {
        // 左手空いていたら
        if (emptyHand == InputManagement.EmptyHand.Left)
        {
            _handPositionDelegate = new HandPositionDelegate(() => _handLeftPosition.position);
        }
        // 右手空いていたら
        else
        {
            _handPositionDelegate = new HandPositionDelegate(() => _handRightPosition.position);
        }
    }

    public void SetMousePositionDelegate()
    {
        Vector3 directionMainCameraLookToBow = _drawObject.position - Camera.main.transform.position;

        _distanceCameraToDrawObject = directionMainCameraLookToBow.magnitude;

        _handPositionDelegate = new HandPositionDelegate(GetMousePos);

        Vector3 GetMousePos()
        {

            // マウスの二次元上の座標
            Vector3 pos = Input.mousePosition;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(pos + Vector3.forward * _distanceCameraToDrawObject);
 
            return worldPos;

        }

    }

    /// <summary>
    /// マウスの三次元の座標を返す
    /// </summary>
    public Vector3 GetMousePosition()
    {
        return _handPositionDelegate();
    }

    /// <summary>
    /// 矢のトランスフォームを設定する
    /// </summary>
    /// <param name="arrow">矢のトランスフォーム</param>
    /// <param name="parent">親になるトランスフォーム</param>
    public void SetArrowFirstTransform(Transform arrow, Transform parent)
    {
        arrow.transform.parent = parent;

        arrow.transform.rotation = _transform.rotation;
    }


    public void WASDMove(Vector3 foward, float moveSpeed)
    {
        foward.y = 0;
        foward = foward.normalized;
        // wasdで移動
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(moveSpeed * Time.deltaTime * foward, Space.World);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(moveSpeed * Time.deltaTime * -(Quaternion.Euler(0f, 90f, 0f) * foward), Space.World);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(moveSpeed * Time.deltaTime * (Quaternion.Euler(0f, 90f, 0f) * foward), Space.World);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(moveSpeed * Time.deltaTime * -foward, Space.World);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            transform.Translate(moveSpeed * Time.deltaTime * Vector3.up, Space.World);
        }
        if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift))
        {
            transform.Translate(moveSpeed * Time.deltaTime * Vector3.down, Space.World);
        }
    }

    public Quaternion GetShotingRotetion()
    {
        return _transform.rotation;
    }
    #endregion
}


