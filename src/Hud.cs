// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using Godot;
using System;

public class Hud : Node2D {

	private AudioStreamGeneratorPlayback gen;
	private float currPhase = 0.0f;

	private TextureRect vignette = new TextureRect() {
		Expand = true,
		Texture = new GradientTexture() {
			Gradient = new Gradient() { Colors = new[] { Colors.Transparent } }
		},
		Material = new ShaderMaterial() {
			Shader = new Shader() {
				Code = @"
					shader_type canvas_item;
					void fragment() {
						float iRad = 0.3;
						float oRad = 1.0;
						float opac = 0.5;
						vec2 uv = SCREEN_UV;
					    vec2 cent = uv - vec2(0.5);
					    vec4 tex = textureLod(SCREEN_TEXTURE, SCREEN_UV, 0.0);
					    vec4 col = vec4(1.0);
					    col.rgb *= 1.0 - smoothstep(iRad, oRad, length(cent));
					    col *= tex;
					    col = mix(tex, col, opac);
					    COLOR = col;
					}"
			}
		}
	};

	public override void _Process(float delta) {
		PrepFrames();
	}

	public override void _Ready() {
		InitVignette();

		var Audio = GetTree().Root.GetNode<Main>("Main").Audio;
		Audio.Stream = new AudioStreamGenerator();
		gen = (AudioStreamGeneratorPlayback)Audio.GetStreamPlayback();
		PrepFrames();
		Audio.Play();
	}

	public override void _EnterTree() {
		var masterIndex = AudioServer.GetBusIndex("Master");
		AudioServer.AddBusEffect(masterIndex, new AudioEffectSpectrumAnalyzer(), 0);
	}

	public override void _Draw() {
		DrawBorder(this);
	}

	private void InitVignette() {
		vignette.RectMinSize = GetViewportRect().Size;
		AddChild(vignette);
		if (Lib.Node.VignetteEnabled) {
			vignette.Show();
		} else {
			vignette.Hide();
		}
	}

	private void PrepFrames() {
		var sampleHz = 44100.0f;
		var periodHz = 3000.0f;

		var incr = (1.0f / (sampleHz / (GetViewport().GetMousePosition().x / GetViewportRect().Size.x * periodHz)));
		var amp = 1.0f - (GetViewport().GetMousePosition().y / GetViewportRect().Size.y);

		var numOfAvailFrames = gen.GetFramesAvailable();
		while (numOfAvailFrames > 0) {
			gen.PushFrame(new Vector2(amp, amp) * (float)Math.Sin(currPhase * (Mathf.Pi * 2)));
			currPhase = (float)(currPhase + incr) % 1;
			numOfAvailFrames -= 1;
		}
	}

	public static void DrawBorder(CanvasItem canvas) {
		if (Lib.Node.BoderEnabled) {
			var vps = canvas.GetViewportRect().Size;
			int thickness = 4;
			var color = new Color(Lib.Node.BorderColorHtmlCode);
			canvas.DrawLine(new Vector2(0, 1), new Vector2(vps.x, 1), color, thickness);
			canvas.DrawLine(new Vector2(1, 0), new Vector2(1, vps.y), color, thickness);
			canvas.DrawLine(new Vector2(vps.x - 1, vps.y), new Vector2(vps.x - 1, 1), color, thickness);
			canvas.DrawLine(new Vector2(vps.x, vps.y - 1), new Vector2(1, vps.y - 1), color, thickness);
		}
	}
}
