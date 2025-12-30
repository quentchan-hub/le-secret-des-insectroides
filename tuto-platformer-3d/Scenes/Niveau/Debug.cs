using Godot;
using System;
using System.Reflection;


public partial class Debug : Node3D
{
	public override void _Ready()
	{
#if DEBUG
		GD.Print("=== DEBUG EXPORT CHECK START ===");
		CheckNodeRecursive(GetTree().CurrentScene);
		GD.Print("=== DEBUG EXPORT CHECK END ===");
#else
		QueueFree();
#endif
	}

	private void CheckNodeRecursive(Node node)
	{
		CheckExportsOnNode(node);

		foreach (Node child in node.GetChildren())
		{
			CheckNodeRecursive(child);
		}
	}

	private void CheckExportsOnNode(Node node)
	{
		var type = node.GetType();

		var fields = type.GetFields(
			BindingFlags.Instance |
			BindingFlags.Public |
			BindingFlags.NonPublic
		);

		foreach (var field in fields)
		{
			if (field.GetCustomAttribute<ExportAttribute>() == null)
				continue;

			var value = field.GetValue(node);

			if (value == null)
			{
				GD.PushWarning(
					$"[DEBUG] Export NON assigné : {node.GetPath()} -> {field.Name}"
				);
			}
		}
	}
}
