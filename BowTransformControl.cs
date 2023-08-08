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
    /// �����I�u�W�F�N�g�̃g�����X�t�H�[���͂���o�R�Ŏ擾
    /// </summary>
    Transform GetDrawObjectTransform { get; }

    /// <summary>
    /// �|�̌����Ă���������擾(world)
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
    #region ����public�������ϐ�

    [SerializeField] Transform _handLeftPosition = default;

    [SerializeField] Transform _handRightPosition = default;
    /// <summary>
    /// �����I�u�W�F�N�g
    /// </summary>
    [SerializeField] Transform _drawObject = default;

    [SerializeField] Transform _changeHandObjectTransform = default;

    [SerializeField] Transform _selectUI;

    [SerializeField] Transform _selectUILeftPosition = default;

    [SerializeField] Transform _selectUIRightPosition = default;

    #endregion

    #region �N���X�A�\����

    delegate Vector3 HandPositionDelegate();


    /// <summary>
    /// ��̏�ԊǗ��^
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
        #region ����������

        _transform = transform;

        if(_drawObject is null)
        {
            _drawObject = this.transform;
        }

        _firstDrawObjectPositon = _drawObject.localPosition;

        _myQuaternion = _transform.localRotation;

        #endregion

    }

    #region public�֐�

    /// <summary>
    /// ���݂̋|���������������Ԃ����
    /// </summary>
    public float GetDrawDistance()
    {
        return Vector3.Distance(_firstDrawObjectPositon, _drawObject.localPosition);
    }

    /// <summary>
    /// ���݂̋|�̈���������(%)
    /// </summary>
    /// <param name="drawLimitDistance">�|�̌��̈�������E����</param>
    /// <param name="drawDistancePercentMaxPower">��̈З͂��ō��ɂȂ鋗��(%)</param>
    public float GetPercentDrawDistance(float drawLimitDistance, float drawDistancePercentMaxPower)
    {
        return Vector3.Magnitude(_drawObject.position - _transform.position) / (drawLimitDistance * drawDistancePercentMaxPower);
    }

    public Transform GetDrawObjectTransform => _drawObject.transform;

    public Vector3 GetHandPosition => _handPositionDelegate();

    /// <summary>
    /// �|�̑O���̃��[���h�����x�N�g�����擾
    /// </summary>
    public Vector3 GetBowFoward()
    {
        return _transform.forward - _transform.position;
    }

    /// <summary>
    /// �͂ނƂ��̃g�����X�t�H�[���ω�
    /// </summary>
    public void GrapSetTransform(Transform arrow)
    {
        arrow.transform.rotation = _transform.rotation;

        arrow.transform.parent = _drawObject.transform;

        arrow.transform.position -= arrow.transform.GetChild(0).position - arrow.transform.position;

        _drawObject.position = _handPositionDelegate();

    }

    /// <summary>
    /// �͂ݒ��̃g�����X�t�H�[���ω�
    /// </summary>
    public void HoldingSetTransform()
    {
        _drawObject.position = _handPositionDelegate();

        // �|�ƌ��̕����x�N�g���ŋ|�̊p�x���߂�AZ�͉�
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
    /// �|��ł�����̃g�����X�t�H�[�����Z�b�g
    /// </summary>
    public void AfterShotResetTransform()
    {

        ResetDrawObject();

        _transform.localRotation = _myQuaternion;

    }

    /// <summary>
    /// ���̈ʒu�����ɂ��ǂ�
    /// </summary>
    public void ResetDrawObject()
    {
        _drawObject.transform.localPosition = _firstDrawObjectPositon;
    }

    /// <summary>
    /// �w�肵����ɉ����ċ|�̃g�����X�t�H�[�����Z�b�g����
    /// </summary>
    /// <param name="emptyHand">��̎�</param>
    public void SetBowTransformInHand(InputManagement.EmptyHand emptyHand)
    {
        // ���肪�󂢂Ă�����
        if (emptyHand == InputManagement.EmptyHand.Left)
        {
            _transform.parent = _handRightPosition;
            _selectUI.parent = _selectUIRightPosition;
            
        }
        // �E�肪�󂢂Ă�����
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
    /// ��̃C���v�b�g�̐ݒ�
    /// </summary>
    public void SetEmptyHandPositionDelegete(InputManagement.EmptyHand emptyHand)
    {
        // ����󂢂Ă�����
        if (emptyHand == InputManagement.EmptyHand.Left)
        {
            _handPositionDelegate = new HandPositionDelegate(() => _handLeftPosition.position);
        }
        // �E��󂢂Ă�����
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

            // �}�E�X�̓񎟌���̍��W
            Vector3 pos = Input.mousePosition;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(pos + Vector3.forward * _distanceCameraToDrawObject);
 
            return worldPos;

        }

    }

    /// <summary>
    /// �}�E�X�̎O�����̍��W��Ԃ�
    /// </summary>
    public Vector3 GetMousePosition()
    {
        return _handPositionDelegate();
    }

    /// <summary>
    /// ��̃g�����X�t�H�[����ݒ肷��
    /// </summary>
    /// <param name="arrow">��̃g�����X�t�H�[��</param>
    /// <param name="parent">�e�ɂȂ�g�����X�t�H�[��</param>
    public void SetArrowFirstTransform(Transform arrow, Transform parent)
    {
        arrow.transform.parent = parent;

        arrow.transform.rotation = _transform.rotation;
    }


    public void WASDMove(Vector3 foward, float moveSpeed)
    {
        foward.y = 0;
        foward = foward.normalized;
        // wasd�ňړ�
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


