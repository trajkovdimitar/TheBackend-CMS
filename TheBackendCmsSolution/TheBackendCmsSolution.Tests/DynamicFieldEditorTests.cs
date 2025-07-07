using Bunit;
using Xunit;
using TheBackendCmsSolution.Web.Components.FieldEditors;

public class DynamicFieldEditorTests : BunitContext
{
    [Fact]
    public void RendersInput_ForString()
    {
        var component = Render<DynamicFieldEditor>(parameters => parameters
            .Add(p => p.FieldName, "Title")
            .Add(p => p.FieldType, "string")
            .Add(p => p.Value, "Hello"));

        Assert.NotNull(component.Find("input"));
    }

    [Fact]
    public void RendersTextArea_ForStringArray()
    {
        var component = Render<DynamicFieldEditor>(parameters => parameters
            .Add(p => p.FieldName, "Tags")
            .Add(p => p.FieldType, "string[]")
            .Add(p => p.Value, new[] { "a", "b" }));

        Assert.NotNull(component.Find("textarea"));
    }
}
