using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PedaleandoGame.Core.Localization;
// using PedaleandoGame.UI.Dialogue; // We'll interop with the existing GDScript DialogueBox
// using PedaleandoGame.Core.Dialogue; // Not needed when using GDScript dialogue dictionaries

namespace PedaleandoGame.Levels
{
	/// <summary>
	/// Manages the first level of the game, including intro sequence and tutorial
	/// Follows Single Responsibility Principle by separating level management from other game logic
	/// </summary>
	public partial class Level1Demo : Node3D
	{
		[Export] public float IntroDuration { get; set; } = 3.0f;

		private Camera3D _introCamera;
		private Camera3D _playerCamera;
		private MeshInstance3D _playerMesh;
		private Node _dialogueBox; // GDScript DialogueBox (res://dialogue_box.gd)
		private CharacterBody3D _player;
		private CanvasLayer _hudCounter;

		private bool _introCompleted;
		private bool _dialogueCompleted;
		private bool _dialogueStarted;

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
			_dialogueBox = GetNode("CanvasLayer2/DialogueBox");
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
			// Primero la animación de cámara; los diálogos comenzarán DESPUÉS
			StartIntroAnimation();
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
			// Use the existing GDScript DialogueBox API: start_dialogue(Array[Dictionary], Callable on_finish)
			var lines = new Godot.Collections.Array<Godot.Collections.Dictionary>
			{
				new Godot.Collections.Dictionary
				{
					{"name", "SALUDOS"},
					{"text", "¡Bienvenido a la costa!"}
				},
				new Godot.Collections.Dictionary
				{
					{"name", "TUTORIAL"},
					{"text", "Usa WASD para moverte y ESPACIO para saltar."}
				},
				new Godot.Collections.Dictionary
				{
					{"name", "TUTORIAL"},
					{"text", "Ahora te encuentras en la playa, rodeado de hermosas palmeras y un sol radiante, pero, necesitamos de tu ayuda para recoger la basura que se encuentra en la playa y el mar... "}
				}
			};

			if (_dialogueBox != null && _dialogueBox.HasMethod("start_dialogue"))
			{
				_dialogueBox.Call("start_dialogue", lines, new Callable(this, nameof(OnDialogueComplete)));
			}
			else
			{
				GD.PushWarning("DialogueBox node missing or does not implement start_dialogue. Skipping tutorial dialogue.");
				OnDialogueComplete();
			}
		}

		private void OnIntroAnimationComplete()
		{
			_introCompleted = true;
			// Iniciar diálogos justo cuando la cámara llega a la vista del jugador
			if (!_dialogueStarted)
			{
				_dialogueStarted = true;
				StartTutorialDialogue();
			}
			else
			{
				TryStartGame();
			}
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
			ShowHUDFade();
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

		private void ShowHUDFade(float duration = 0.6f)
		{
			if (_hudCounter == null)
				return;

			// Ensure visible before fade-in
			_hudCounter.Visible = true;

			foreach (var item in EnumerateCanvasItems(_hudCounter))
			{
				var start = item.Modulate;
				start.A = 0.0f;
				var end = new Color(start.R, start.G, start.B, 1.0f);
				item.Modulate = start;

				var t = CreateTween();
				t.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
				t.TweenProperty(item, "modulate", end, duration);
			}
		}

		private IEnumerable<CanvasItem> EnumerateCanvasItems(Node parent)
		{
			foreach (var child in parent.GetChildren())
			{
				if (child is CanvasItem ci)
					yield return ci;
				if (child.GetChildCount() > 0)
				{
					foreach (var nested in EnumerateCanvasItems(child))
						yield return nested;
				}
			}
		}

		#endregion
	}

	// Using shared DialogueLine from Core.Dialogue
}
