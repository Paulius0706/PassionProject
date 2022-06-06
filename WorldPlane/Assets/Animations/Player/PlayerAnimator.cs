using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerAnimator : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public ItemHolder itemHolder;
    private Animator animator;
    

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (animator != null)
        {
            if(itemHolder.itemType == ItemHolder.ItemType.sword)
            {
                if (itemHolder.itemIsActive) { animator.SetBool("SwordAttack", true); }
                else { animator.SetBool("SwordAttack", false); }
                string state = "";
                state = "SwordPreperation";
                if (isPlaying(animator, state) && !isPlayingLoop(itemHolder.itemAnimator, state))
                {
                    itemHolder.itemAnimator.Play(state);
                }
                state = "Attack";
                if (isPlaying(animator, state) && !isPlayingLoop(itemHolder.itemAnimator, state))
                {
                    itemHolder.itemAnimator.Play(state);
                }
                state = "BackAttack";
                if (isPlaying(animator, state) && !isPlayingLoop(itemHolder.itemAnimator, state))
                {
                    itemHolder.itemAnimator.Play(state);
                }
                state = "BackAttack";
                if (isPlaying(animator, state) && !isPlayingLoop(itemHolder.itemAnimator, state))
                {
                    itemHolder.itemAnimator.Play(state);
                }
                state = "RetrackSword";
                if (isPlaying(animator, state) && !isPlayingLoop(itemHolder.itemAnimator, state))
                {
                    itemHolder.itemAnimator.Play(state);
                }
                state = "RetrackBackSword";
                if (isPlaying(animator, state) && !isPlayingLoop(itemHolder.itemAnimator, state))
                {
                    itemHolder.itemAnimator.Play(state);
                }
                state = "Idle";
                if (isPlaying(animator, state) && !isPlayingLoop(itemHolder.itemAnimator, state))
                {
                    itemHolder.itemAnimator.Play(state);
                }

            }
            else { animator.SetBool("SwordAttack", false); }
        }
    }
    bool isPlaying(Animator anim, string stateName)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
        anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            return true;
        return false;
    }
    bool isPlayingLoop(Animator anim, string stateName)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(stateName)) return true;
        return false;
    }
}
