using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerController : PlayerController
{
    #region Fields

    [SerializeField] VirtualJoystick _moveJoystick;

    private bool _canDash = true;
    private bool _canAttack = true;
    private Dictionary<int, bool> canSkill = new Dictionary<int, bool>();

    #endregion

    void Start()
    {
        Init();
    }

    protected override void Init()
    {
        base.Init();
        _moveJoystick = GameObject.Find("MoveJoystick").GetComponent<VirtualJoystick>();
        canSkill.Add(FirstSkillId, true);
        canSkill.Add(SecondSkillId, true);
        GameObject.Find("FirstSkillButton").GetComponent<FirstSkillButton>().Init();
        GameObject.Find("SecondSkillButton").GetComponent<SecondSkillButton>().Init();
    }

    protected override void Update()
    {
        UpdateMove();
        UpdateAnimation();
    }

    protected override void UpdateMove()
    {
        if (!(State == ActionState.Idle || State == ActionState.Run))
            return;

        if (_moveJoystick._handleDir == Vector2.zero)
        {
            State = ActionState.Idle;
            CheckUpdatedFlag();
            return;
        }

        _destPos = (Vector2)transform.position;

        if (_moveJoystick._handleDir != Vector2.zero)
        {
            State = ActionState.Run;
            _destPos += _moveJoystick._handleDir * 1.5f * Time.deltaTime;
            LastDir = _moveJoystick._handleDir;
        }

        UpdateDir(_destPos);
        transform.position = _destPos;
        Pos = _destPos;

        SetLayer(_sprite);

        CheckUpdatedFlag();
    }

    public override bool Attack()
    {
        if (_canAttack == false)
            return false;

        if (!(State == ActionState.Idle || State == ActionState.Run))
            return false;

        _coroutine = StartCoroutine(CoAttack());
        return true;
    }

    protected override IEnumerator CoAttack()
    {
        if (!(State == ActionState.Idle || State == ActionState.Run))
            yield break;

        if (_canAttack == false || LastDir == Vector2.zero)
            yield break;

        _canAttack = false;
        float time = 0;

        foreach (var anim in _animationClips)
        {
            if (anim.name == $"{_directionY.ToString()}Attack")
                time = anim.length;
        }

        State = ActionState.Attack;

        C_Attack attackPacket = new C_Attack();
        Managers.Network.Send(attackPacket);

        yield return new WaitForSeconds(time);

        State = ActionState.Idle;
        CheckUpdatedFlag();
        StartCoroutine(CoAttackCooldown());
    }

    public bool Dash()
    {
        if (_canDash == false)
            return false;

        if ((_moveJoystick._handleDir == Vector2.zero && LastDir == Vector2.zero))
            return false;

        if (!(State == ActionState.Idle || State == ActionState.Run))
            return false;

        _coroutine = StartCoroutine(CoDash());
        return true;
    }

    public IEnumerator CoDash()
    {
        _canDash = false;
        Vector2 dir;

        if (_moveJoystick._handleDir == Vector2.zero)
            dir = LastDir;
        else
            dir = _moveJoystick._handleDir;

        State = ActionState.Dash;
        float dashTime = 0.5f;

        while (dashTime > 0)
        {
            _destPos = (Vector2)transform.position;
            _destPos += dir * 10f * Time.deltaTime;
            transform.position = _destPos;
            Pos = _destPos;
            CheckUpdatedFlag();
            dashTime -= Time.deltaTime;
            yield return null;
        }

        State = ActionState.Idle;
        CheckUpdatedFlag();
        StartCoroutine(CoDashCooldown());
    }

    protected override IEnumerator CoDie(S_Die diePacket)
    {
        Camera.main.transform.SetParent(null);
        Managers.Game.Rank = diePacket.Rank;
        Managers.Object.RemovePlayer(this.gameObject);

        GameObject go = Managers.UI.GenerateUI("UI/GameEnd");

        if (diePacket.Rank == 1)
            go.GetComponent<GameEnd>().Exit();
        else
            go.GetComponent<GameEnd>().Monitor();

        State = ActionState.Dead;
        yield return new WaitForSeconds(3.0f);

        GameObject controlUI = GameObject.Find("ControlUI");
        controlUI.SetActive(false);

        Clear();
    }

    public override bool UsingSkill(int skillId)
    {
        if (canSkill[skillId] == false)
            return false;

        if (!(State == ActionState.Idle || State == ActionState.Run))
            return false;

        canSkill[skillId] = false;

        C_UseSkill skillPacket = new C_UseSkill();
        skillPacket.SkillId = skillId;
        Managers.Network.Send(skillPacket);

        base.UsingSkill(skillId);
        StartCoroutine(CoSkillCooldown(skillId, Managers.Data.SkillData[skillId].Cooldown));
        return true;
    }

    public void EndGame()
    {
        GameObject go = Managers.UI.GenerateUI("UI/GameEnd");
        go.GetComponent<GameEnd>().Exit();
    }

    IEnumerator CoAttackCooldown()
    {
        yield return new WaitForSeconds(Managers.Data.ClassData[Class.ToString()].AttackDelay);
        _canAttack = true;
    }

    IEnumerator CoDashCooldown()
    {
        yield return new WaitForSeconds(Managers.Data.ClassData[Class.ToString()].DashCooldown);
        _canDash = true;
    }

    IEnumerator CoSkillCooldown(int skillId, float delay)
    {
        yield return new WaitForSeconds(delay);
        canSkill[skillId] = true;
    }

    void CheckUpdatedFlag()
    {
        if (_updated)
        {
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = PosInfo;
            Managers.Network.Send(movePacket);
            _updated = false;
        }
    }
}
