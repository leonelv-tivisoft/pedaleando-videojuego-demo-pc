using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PedaleandoGame.Core.Localization;
using PedaleandoGame.UI.Dialogue;
using PedaleandoGame.Core.Dialogue;

namespace PedaleandoGame.Levels
{
	/// <summary>
	/// Manages the first level of the game, including intro sequence and tutorial
	/// Follows Single Responsibility Principle by separating level management from other game logic
	/// </summary>
	public partial class Level1Demo : Node3D
	{
		[Export] private float IntroDuration { get; set; } = 3.0f;

		private Camera3D _introCamera;
		private Camera3D _playerCamera;
		private MeshInstance3D _playerMesh;
		private DialogueBox _dialogueBox;
		private CharacterBody3D _player;
		private CanvasLayer _hudCounter;

		private bool _introCompleted;
		private bool _dialogueCompleted;

		public override void _Ready()
		{
			InitializeComponents();
			SetupInitialState();
			StartIntroSequence();
		}

		#region Initialization

		private void InitializeComponents()
		{
			_introCamera = GetNode<Camera3D>("IntroCamera");
			_playerCamera = GetNode<Camera3D>("Player/Pivot/PlayerCamera");
			_playerMesh = GetNode<MeshInstance3D>("Player/MeshInstance3D");
			_dialogueBox = GetNode<DialogueBox>("CanvasLayer2/DialogueBox");
			_player = GetNode<CharacterBody3D>("Player");
			_hudCounter = GetNode<CanvasLayer>("Contador");
		}

		private void SetupInitialState()
		{
			SetupCameras();
			DisablePlayerControl();
			HideHUD();
		}

		private void SetupCameras()
		{
			_introCamera.Current = true;
			_playerCamera.Current = false;
			_playerMesh.Visible = true;
		}

		private void DisablePlayerControl()
		{
			_player.SetPhysicsProcess(false);
			_player.SetProcessInput(false);
			_player.SetProcessUnhandledInput(false);
		}

		private void HideHUD()
		{
			if (_hudCounter != null)
			{
				_hudCounter.Visible = false;
			}
		}

		#endregion

		#region Intro Sequence

		private void StartIntroSequence()
		{
			StartIntroAnimation();
			StartTutorialDialogue();
		}

		private void StartIntroAnimation()
		{
			var tween = CreateTween();
			
			// Camera position animation
			tween.TweenProperty(
				_introCamera, 
				"global_position", 
				_playerCamera.GlobalPosition, 
				IntroDuration
			).SetTrans(Tween.TransitionType.Sine)
			 .SetEase(Tween.EaseType.InOut);

			// Camera rotation animation
			tween.TweenProperty(
				_introCamera, 
				"global_rotation", 
				_playerCamera.GlobalRotation, 
				IntroDuration
			).SetTrans(Tween.TransitionType.Sine)
			 .SetEase(Tween.EaseType.InOut);

			tween.Finished += OnIntroAnimationComplete;
		}

		private void StartTutorialDialogue()
		{
			var dialogueLines = new[]
			{
				new DialogueLine(
					LocalizationManager.Instance.GetText("DIALOG_TITLE_GREETING"),
					LocalizationManager.Instance.GetText("WELCOME_MESSAGE")),
				new DialogueLine(
					LocalizationManager.Instance.GetText("DIALOG_TITLE_TUTORIAL"),
					LocalizationManager.Instance.GetText("TUTORIAL_MOVE")),
				new DialogueLine(
					LocalizationManager.Instance.GetText("DIALOG_TITLE_TUTORIAL"),
					LocalizationManager.Instance.GetText("TUTORIAL_OBJECTIVE"))
			};

			_dialogueBox.StartDialogue(dialogueLines, OnDialogueComplete);
		}

		private void OnIntroAnimationComplete()
		{
			_introCompleted = true;
			TryStartGame();
		}

		private void OnDialogueComplete()
		{
			_dialogueCompleted = true;
			TryStartGame();
		}

		#endregion

		#region Game Start

		private void TryStartGame()
		{
			if (!(_introCompleted && _dialogueCompleted))
				return;

			TransitionToGameCamera();
			EnablePlayerControl();
			ShowHUD();
		}

		private void TransitionToGameCamera()
		{
			// Smooth camera transition
			_introCamera.GlobalTransform = _playerCamera.GlobalTransform;
			_playerCamera.Current = true;
			_introCamera.Current = false;

			// Update player view
			_playerMesh.Visible = false;
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}

		private void EnablePlayerControl()
		{
			_player.SetPhysicsProcess(true);
			_player.SetProcessInput(true);
			_player.SetProcessUnhandledInput(true);
		}

		private void ShowHUD()
		{
			if (_hudCounter != null)
			{
				_hudCounter.Visible = true;
			}
		}

		#endregion
	}

	// Using shared DialogueLine from Core.Dialogue
}
