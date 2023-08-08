// --------------------------------------------------------- 
// CanDraw_BowManager.cs 
// 
// CreateDay: 
// Creator  : 
// --------------------------------------------------------- 
using UnityEngine;
[RequireComponent(typeof(BowTransformControl))]
public abstract class CanDraw_BowManager : BowManager
{
    #region variable 

    protected Transform _drawObject = default;

    protected IFBowTransformControl _transformControl = default;

    protected Quaternion _machineGunRotation = default;

    #region �p�����[�^

    /// <summary>
    /// ���͂߂錷����̋���
    /// </summary>
    [SerializeField] protected float grapLimitDistance = 0.5f;

    /// <summary>
    /// ����������E�p�x
    /// </summary>
    [SerializeField] protected float drawLimitAngle = 90f;

    /// <summary>
    /// ����������E����
    /// </summary>
    [SerializeField] protected float drawLimitDistance = 5f;

    /// <summary>
    /// �A�˖h�~
    /// </summary>
    [SerializeField] protected float cantShotDistance = 0.01f;

    /// <summary>
    /// ���������Ƀ}�b�N�X�p���[�ɂȂ鋗���̍ō������̊���
    /// </summary>
    [SerializeField] protected float drawDistancePercentMaxPower = 0.9f;

    const int BACK = -1;
    #endregion

    #endregion
    #region property
    protected override Transform GetSpawnPosition => _drawObject;

    protected override Vector3 GetShotDirection => _transformControl.GetBowFoward();


    #endregion
    #region method

    // �����������Q
    protected override void Start()
    {

        _transformControl = GetComponent<BowTransformControl>();

        CastInterface();

        _drawObject = _transformControl.GetDrawObjectTransform;

        base.Start();
    }

    // �͂ގ��ɌĂ΂��
    protected override void ProcessOfGrapObject()
    {
        // ����͂߂�͈͓��Ȃ�
        if (grapLimitDistance > Vector3.Distance(_drawObject.position, _transformControl.GetHandPosition))
        {
            BowStringGrap();
        }
    }
    /// <summary>
    /// �|�̌���͂ޏ����Q
    /// </summary>
    protected virtual void BowStringGrap()
    {

        BowGrapCreateArrow(_drawObject.position);
        _transformControl.GrapSetTransform(_arrow.transform);
    }

    // �|�̌����������Q
    protected virtual void BowStringHold()
    {
        // �����Ă��鎞�̃g�����X�t�H�[���Z�b�g
        _transformControl.HoldingSetTransform();

        BowHoldingSetAttract();
 
        // �~���͈͊O�܂ň����Ƌ����I�ɖ������
        if (!ConeDecision.ConeInObject(transform,_drawObject, drawLimitAngle, drawLimitDistance, BACK))
        {
            ProcessOfReleaseObjcect();
        }
    }

    /// <summary>
    /// �|�̌������Q
    /// </summary>
    protected virtual void BowShotStart()
    {
        
        // �������
        BowShotSetting(GetShotDirection);

       
    }

    sealed protected override void BowShotArrow(Vector3 shotDirection)
    {
        // �A�˖h�~
        if (_transformControl.GetDrawDistance() < cantShotDistance && !_canMachineGun)
        {
            _playerManager.ResetArrow();
            _playerManager.CanRapid = false;
            _canMachineGun = false;
            X_Debug.Log(_transformControl.GetDrawDistance() + "," + cantShotDistance + "aaa");
            return;
        }
        else
        {
            _transformControl.ResetDrawObject();
            base.BowShotArrow(shotDirection);

        }
        // �}�V���K���łȂ���΃g�����X�t�H�[���Z�b�g
        if (!_canMachineGun)
        {
            // �����������̃g�����X�t�H�[���ݒ�
            _transformControl.AfterShotResetTransform();
        }
    }

    /// <summary>
    /// �ǉ��s��
    /// </summary>
    protected abstract void AddBowShotProcess();

    /// <summary>
    /// �C���^�[�t�F�[�X�^���g�p���邽�߂��L���X�g���邽�߂̃��\�b�h
    /// �x�[�X���Q�b�g�R���|�[�l���g��ɌĂ΂��
    /// </summary>
    protected abstract void CastInterface();
    #endregion
}