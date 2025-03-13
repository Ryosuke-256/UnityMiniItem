using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

sealed class PenlightNear : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Mesh _mesh = null; //��ʕ`�悷��mesh
    [SerializeField] Material _material = null; //��ʕ`�悷��material
    [SerializeField] PenlightNearAnimation _audience = PenlightNearAnimation.Default(); //�A�j���[�V�������ʕ`��͂�����Ő���
    public AudioGain _audiolevel; //����1
    public ClickGain _clickelevel; //����2

    #endregion

    #region Private objects
    //GPU�����_�����O���g�����߂Ƀ������̊m�ۂ��蓮�ōs��
    NativeArray<Matrix4x4> _matrices;
    NativeArray<Color> _colors;
    GraphicsBuffer _colorBuffer;
    MaterialPropertyBlock _matProps;
    private float _phase; //�y�����C�g�̐U��̈ʑ�

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        //������
        _matrices = new NativeArray<Matrix4x4>
          (_audience.TotalSeatCount, Allocator.Persistent,
           NativeArrayOptions.UninitializedMemory);

        _colors = new NativeArray<Color>
          (_audience.TotalSeatCount, Allocator.Persistent,
           NativeArrayOptions.UninitializedMemory);

        _colorBuffer = new GraphicsBuffer
          (GraphicsBuffer.Target.Structured,
           _audience.TotalSeatCount, sizeof(float) * 4);

        _matProps = new MaterialPropertyBlock();

        _phase = 0;

    }

    void Update()
    {
        /*
        ���͒l�͂������� [0,1]�ɂ��Ă����B�A�j���[�V�����̋����Ƃ��āA臒l�𒴂�����A�j���[�V�������n�܂�悤�Ȃ��̂�
        �z�肵�Ă���̂ŁA�ŏ��l�ƍő�����߂�B
        */
        float min = 0.05f, max = 0.8f;
        //var displacement = (Mathf.Sin(Time.time * math.PI * 2.0f) + 1.0f) / 2.0f;
        float displacement = math.max(_clickelevel.GetClickLevel(), _audiolevel.GetAudioLevel());
        var clipeddisplacement = math.clamp(displacement, min, max);
        /*
         * �y�����C�g�̑����A�j���[�V�����Ƃƒx���A�j���[�V������p�ӂ��A���͒l�ɂ�肻�������ւ���B
         * �X�C�b�`����ۂ͈ʑ����A���I�ɕω����Ăق����̂�Lerp()���g���Đ��`�⊮����B
         */
        var freq_f = 2.0f * math.PI * 5.0f * Time.deltaTime;
        var freq_s = 2.0f * math.PI * 0.5f * Time.deltaTime;
        _phase += math.lerp(freq_s, freq_f, clipeddisplacement);

        // PenlightAnimation�ɓn���l
        var job = new PenlightNearAnimationJob()
        {
            config = _audience,
            xform = transform.localToWorldMatrix,
            time = Time.time,
            volume = displacement,
            phase = _phase,
            matrices = _matrices,
            colors = _colors
        };
        job.Schedule(_audience.TotalSeatCount, 64).Complete();

        _colorBuffer.SetData(_colors);
        _material.SetBuffer("_InstanceColorBuffer", _colorBuffer);

        //GPU�����_�����O
        var rparams = new RenderParams(_material) { matProps = _matProps };
        var (i, step) = (0, _audience.BlockSeatCount);
        for (var sx = 0; sx < _audience.blockCount.x; sx++)
        {
            for (var sy = 0; sy < _audience.blockCount.y; sy++, i += step)
            {
                _matProps.SetInteger("_InstanceIDOffset", i);
                Graphics.RenderMeshInstanced
                  (rparams, _mesh, 0, _matrices, step, i);
            }
        }
    }

    #endregion
    //�I������
    void OnDestroy()
    {
        _matrices.Dispose();
        _colors.Dispose();
        _colorBuffer.Dispose();
    }
}
