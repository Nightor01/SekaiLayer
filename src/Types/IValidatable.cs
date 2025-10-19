namespace SekaiLayer.Types;

public interface IValidatable
{
    public bool Validate();
    public object GetData();
}