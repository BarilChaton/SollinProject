using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponControl : MonoBehaviour
{

    [SerializeField] private GameObject weapon;
    private bool canAttack = true;
    [SerializeField] private float attackCooldown = 0.5f;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            WeaponAttack();
        }
    }

    public void WeaponAttack()
    {
        if (canAttack)
        {
            canAttack = false;
            Animator anim = weapon.GetComponent<Animator>();
            anim.SetTrigger("Attack");
            StartCoroutine(ResetAttackCooldown());
        }
        //else if(attackCooldown >= 0.5f)
        //{
        //    StopAllCoroutines();
        //}
    }

    IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}
