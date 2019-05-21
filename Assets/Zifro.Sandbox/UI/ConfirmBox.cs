﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zifro.Sandbox.Utility;

namespace Zifro.Sandbox.UI
{
	public class ConfirmBox : MonoBehaviour
	{
		public Text labelText;
		public Button confirmButton;
		public Button abortButton;
		public GameObject hideOnClicked;

		Action confirmCallback;
		Action abortCallback;

		void Start()
		{
			Debug.Assert(labelText, $"{nameof(labelText)} is not assigned for '{name}'.", this);
			Debug.Assert(confirmButton, $"{nameof(confirmButton)} is not assigned for '{name}'.", this);
			Debug.Assert(abortButton, $"{nameof(abortButton)} is not assigned for '{name}'.", this);
			Debug.Assert(hideOnClicked, $"{nameof(hideOnClicked)} is not assigned for '{name}'.", this);
		}

		void OnEnable()
		{
			if (confirmCallback == null && abortCallback == null)
			{
				HideBox();
			}

			confirmButton.onClick.AddListener(OnConfirmButton);
			abortButton.onClick.AddListener(OnAbortButton);
			hideOnClicked.AddTrigger(EventTriggerType.PointerClick, OnDeselect);
		}

		void OnConfirmButton()
		{
			confirmCallback?.Invoke();
			HideBox();
		}

		void OnAbortButton()
		{
			abortCallback?.Invoke();
			HideBox();
		}

		void OnDeselect(BaseEventData arg0)
		{
			HideBox();
		}

		public void ShowBox(string label, Action onConfirm, Action onAbort)
		{
			labelText.text = label;
			confirmCallback = onConfirm;
			abortCallback = onAbort;
			gameObject.SetActive(true);
			EventSystem.current.SetSelectedGameObject(abortButton.gameObject);
		}

		public void HideBox()
		{
			gameObject.SetActive(false);
			confirmCallback = null;
			abortCallback = null;
		}

	}
}
