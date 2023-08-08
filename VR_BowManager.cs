// --------------------------------------------------------- 
// ArrowPassiveEffect.cs 
// 
// CreateDay: 2023/06/08
// Creator  : Sasaki
// --------------------------------------------------------- 

using UnityEngine;

[RequireComponent(typeof(BowVibe), typeof(AttractEffectCustom), typeof(AttractZone))]
[RequireComponent(typeof(Inhall))]
public class VR_BowManager : CanDraw_BowManager
{

    #region ����public�������ϐ�

    [SerializeField] Transform _changeHandObjectTransform = default;

    [SerializeField] SceneObject scene = default;

    /// <summary>
    /// VR�Œl�ǂ����[�h�A�f�o�b�O�p
    /// </summary>
    public bool _traceValue = false;

    #endregion

    #region �N���X�A�\����

    private InputManagement _vrInput = default;

    private  new IFBowTransformControl_Bow _transformControl = default;

    private VR_Trace_Value _trace = default;

    private IFBowVibe _vibe = default;

    #endregion

    const float CHANGE_COOL_TIME = 3f;

    float cacheTime = 0f;

    bool isTitle = false;

    protected override void Start()
    {
        isTitle = IsTitleScene();

        _vibe = GetComponent<BowVibe>();

        _vrInput = GameObject.FindGameObjectWithTag(_InputTagName.TagName).GetComponent<InputManagement>();

        base.Start();

        _transformControl = base._transformControl as IFBowTransformControl_Bow;

        SetHandTransform();

        // �f�o�b�O�p
        if (_traceValue)
        {
            _trace = this.gameObject.AddComponent<VR_Trace_Value>();
        }
    }

    private void Update()
    {
        BowUpdateCallProcess();
    }

    protected override void ProcessOfGrapObject()
    {
        
        // �肪�|�̌��̋߂��ɂ���ꍇ����
        if (grapLimitDistance > Vector3.Distance(_drawObject.position, _transformControl.GetHandPosition))
        {
            BowStringGrap();
        }
        // �肪�A�|�̎���̐؂�ւ���|�W�V�����̋߂��ɂ���ꍇ
        else if (grapLimitDistance > Vector3.Distance(_changeHandObjectTransform.position, _transformControl.GetHandPosition) && CanHandChange())
        {
            ChangeHand();
        }
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
        SetHandUseDelegate();
    }

    protected override float GetShotPercentPower()
    {
        return _transformControl.GetPercentDrawDistance(drawLimitDistance, drawDistancePercentMaxPower);
    }

    protected override void BowStringGrap()
    {      

        base.BowStringGrap();
    }

    /// <summary>
    /// �|�̌��͂ݒ�
    /// </summary>
    protected override void BowStringHold()
    {
        _vibe.HoldingVibe(_percentDrawPower);
        base.BowStringHold();
    }

    /// <summary>
    /// �|��������
    /// </summary>
    protected override void  BowShotStart()
    {
        _vibe.EndDrawVibe();

        base.BowShotStart();

    }


    protected override void AddBowShotProcess()
    {
        _vibe.StartShotVibe(_percentDrawPower);
    }

    protected override void CastInterface()
    {
        _transformControl = base._transformControl as IFBowTransformControl_Bow;
    }


    /// <summary>
    /// ��̐؂�ւ�
    /// </summary>
    private void ChangeHand()
    {
        // ����̏ꍇ
        if (_vrInput.P_EmptyHand == InputManagement.EmptyHand.Left)
        {
            _vrInput.P_EmptyHand = InputManagement.EmptyHand.Right;
        }
        // �E��̏ꍇ
        else
        {
            _vrInput.P_EmptyHand = InputManagement.EmptyHand.Left;

        }

        SetHandTransform();
    }

    private void SetHandTransform()
    {
        _transformControl.SetBowTransformInHand(_vrInput.P_EmptyHand);

        SetHandUseDelegate();
    }

    /// <summary>
    /// �|�̎����Ă����ɂ��C���v�b�g�̐ݒ�
    /// </summary>
    private void SetHandUseDelegate()
    {

        // �����
        if (_vrInput.P_EmptyHand == InputManagement.EmptyHand.Left)
        {
            _grapTriggerInput = () => _vrInput.ButtonLeftUpTrigger();

            _releaseTriggerInput = () => !_vrInput.ButtonLeftUpTrigger();

            _vibe.SetRightShotAction();

        }
        // �E���
        else
        {
            _grapTriggerInput = () => _vrInput.ButtonRightUpTrigger();

            _releaseTriggerInput = () => !_vrInput.ButtonRightUpTrigger();

            _vibe.SetLeftShotAction();
        }

        _transformControl.SetEmptyHandPositionDelegete(_vrInput.P_EmptyHand);
    }

    private bool IsTitleScene()
    {
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == scene.SceneName)
        {
            return true;
        }

        return false;
    }

    private bool CanHandChange()
    {
        if (!isTitle)
        {
            return false;
        }

        if(Time.time - cacheTime > CHANGE_COOL_TIME)
        {
            cacheTime = Time.time;
            return true;
        }

        return false;
    }
    /// <summary>
    /// �f�o�b�O�p
    /// </summary>
    /// <param name="text"></param>
    private void SetText(string text)
    {
        if (_traceValue)
        {
            _trace.SetText(text);
        }
        else
        {
            X_Debug.Log("�l�ǂ����[�h����");
        }

    }


}
