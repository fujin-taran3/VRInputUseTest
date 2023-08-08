// --------------------------------------------------------- 
// ArrowPassiveEffect.cs 
// 
// CreateDay: 2023/06/08
// Creator  : Sasaki
// --------------------------------------------------------- 
using System;
using UnityEngine;
using System.Collections;

public interface IFBowManagerQue
{
    /// <summary>
    /// 矢をキューにいれる
    /// </summary>
    /// <param name="arrow">キューにいれるオブジェクト</param>
    public void ArrowQue(CashObjectInformation arrow);

    /// <summary>
    /// 矢の連射イベント通知
    /// </summary>
    public void SetArrowMachineGun(int arrowValue, float delayTime);
}

public interface IFBowManagerUpdate
{
    /// <summary>
    /// 弓を動かすためにupdateでよぶ
    /// ProcessOfから始まる名前のメソッドのいずれかをよぶ
    /// </summary>
    public void BowUpdateCallProcess();
}

public interface IFBowManager_GetStats
{
    public bool IsHolding { get; }
}
[RequireComponent(typeof(Inhall), typeof(AttractEffectCustom), typeof(AttractZone))]

public abstract class BowManager : MonoBehaviour, IFBowManagerQue, IFBowManagerUpdate, IFBowManager_GetStats
{
    #region かつてpublicだった変数
    [SerializeField] protected TagObject _InputTagName = default;

    [SerializeField] protected TagObject _poolTagName = default;

    [SerializeField] protected TagObject _playerManagerTagName = default;

    #endregion

    #region クラス、構造体

    protected IFAttractEffectCustom _inhallCustom = default;

    protected AttractZone _attract = default;

    protected ObjectPoolSystem _poolSystem = default;

    protected IFPlayerManagerShotArrow _playerManager = default;

    protected CashObjectInformation _arrow = default;

    protected IFBowSE_CallToBow _bowSE = default;

    protected Func<bool> _grapTriggerInput = default;

    protected Func<bool> _releaseTriggerInput = default;


    protected bool _canMachineGun;

    protected int _valueMachineGun;

    protected WaitForSeconds _delayTime;

    float _setedArrowSpeed = 0f;



    /// <summary>
    /// 手の状態管理型
    /// </summary>
    protected enum HandStats
    {
        None,
        Hold,
    }

    protected HandStats _handStats = HandStats.None;

    #endregion

    #region パラメータ

    /// <summary>
    /// 弦引いたパワーに掛ける力の強さ
    /// </summary>
    [SerializeField] float arrowSpeed = 2000f;

    protected float _percentDrawPower = 0f;

    Vector3 _shotStartRoteCache = Vector3.zero;

    #endregion

    public bool IsHolding
    {
        get
        {
            return _handStats == HandStats.Hold;
        }
    }

    protected abstract Transform GetSpawnPosition { get; }

    protected abstract Vector3 GetShotDirection { get; }


    /// <summary>
    /// 必須変数のゲットコンポーネントとSetInputDelegate()を行う
    /// </summary>
    protected virtual void Start()
    {
        #region 初期化たち

        _attract = GetComponent<AttractZone>();

        _inhallCustom = GetComponent<AttractEffectCustom>();

        _playerManager = GameObject.FindGameObjectWithTag(_playerManagerTagName.TagName).GetComponent<PlayerManager>();

        _poolSystem = GameObject.FindGameObjectWithTag(_poolTagName.TagName).GetComponent<ObjectPoolSystem>();

        _bowSE = GetComponent<BowSE>();

        #endregion

        // インプットの設定
        SetInputDelegate();
    }

    /// <summary>
    /// これを呼んでくれないと働いてくれない基本行動
    /// </summary>
    public void BowUpdateCallProcess()
    {
        // 物を掴む入力があった場合
        if ((_grapTriggerInput() && _handStats == HandStats.None) && !_canMachineGun)
        {
            // 掴む
            ProcessOfGrapObject();
        }
        // 何かしら掴んでいた場合
        else if (_handStats == HandStats.Hold)
        {
            // 引いたパワー(%)で取得(0-1)
            _percentDrawPower = GetShotPercentPower();

            if (!_releaseTriggerInput())
            {
                // 物を掴み続ける
                ProcessOfHoldObject();

                return;
            }
            // 物を離す
            ProcessOfReleaseObjcect();
        }

    }


    /// <summary>
    /// 矢をキューにいれる
    /// </summary>
    /// <param name="arrow">キューにいれるオブジェクト</param>
    public void ArrowQue(CashObjectInformation arrow)
    {
        _poolSystem.ReturnObject(arrow);
    }

    /// <summary>
    /// 物を掴む処理
    /// 掴む操作をして何ももってないと呼ばれる
    /// </summary>
    protected abstract void ProcessOfGrapObject();

    /// <summary>
    /// 物を掴み中の時の処理
    /// 掴む操作を続けている間呼ばれ続ける
    /// </summary>
    protected abstract void ProcessOfHoldObject();


    /// <summary>
    /// 物を離す時の処理
    /// 掴む操作を止めると呼ばれる
    /// </summary>
    protected abstract void ProcessOfReleaseObjcect();

    /// <summary>
    /// 弓の弦を掴み開始時の必須処理、矢を生成
    /// </summary>
    protected void BowGrapCreateArrow(Vector3 spawnPosition)
    {

        _bowSE.CallDrawStart();

        _bowSE.CallAttractStartSE();

        // 矢を生成
        _arrow = _poolSystem.CallObject(PoolEnum.PoolObjectType.arrow, spawnPosition);

        _handStats = HandStats.Hold;
    }

    /// <summary>
    /// 弓の弦を掴み中必須処理、吸込みを有効化
    /// </summary>
    protected void BowHoldingSetAttract()
    {
        _bowSE.CallAttractSE(_percentDrawPower);

        _bowSE.CallDrawingSE(_percentDrawPower);

        // エフェクトを再生
        _inhallCustom.SetActive(true);

        // エフェクトの動的加工
        _inhallCustom.SetEffectSize(_percentDrawPower);

        // 吸込み判定を弓を引いた量によって変える
        _attract.SetAngle(_percentDrawPower);

    }


    /// <summary>
    /// 射撃時の必須処理
    /// </summary>
    protected void BowShotSetting(Vector3 shotDirection)
    {

        // 再生されていたエフェクトを無効
        _inhallCustom.SetActive(false);
        _setedArrowSpeed = _percentDrawPower * arrowSpeed;
        _shotStartRoteCache = shotDirection;
        BowShotArrow(shotDirection);

        // 吸込み判定初期化
        _attract.SetAngle(0f);

        _handStats = HandStats.None;
    }

    /// <summary>
    /// 撃てるなら撃つ
    /// </summary>
    /// <param name="shotDirection"></param>
    protected virtual void BowShotArrow(Vector3 shotDirection)
    {
        _bowSE.CallShotSE();

        // 矢のスピードセット、弓を引いた量によって変える
        _playerManager.SetArrowMoveSpeed(_setedArrowSpeed);

        // 矢を撃つ
        _playerManager.ShotArrow(shotDirection);

        if (_canMachineGun && --_valueMachineGun > 0)
        {
            StartCoroutine(MachineGun());

        }
        else if (_valueMachineGun <= 0)
        {
            _playerManager.CanRapid = false;
            _canMachineGun = false;
        }


    }

    protected virtual IEnumerator MachineGun()
    {
        yield return _delayTime;
        _poolSystem.CallObject(PoolEnum.PoolObjectType.arrow, GetSpawnPosition.position);
        yield return null;

        BowShotArrow(GetShotDirection);
    }

    /// <summary>
    /// インプットの設定、物を掴む、離すの操作を設定する
    /// 最初に呼ばれる
    /// </summary>
    protected abstract void SetInputDelegate();

    /// <summary>
    /// 弓の引いたパワー(%)を取得
    /// 物を掴み中に呼ばれ続ける
    /// </summary>
    /// <returns></returns>
    protected abstract float GetShotPercentPower();

    public void SetArrowMachineGun(int arrowValue, float delayTime)
    {
        _canMachineGun = true;
        _valueMachineGun = arrowValue;
        _delayTime = new WaitForSeconds(delayTime);
        _playerManager.CanRapid = true;
    }

}
