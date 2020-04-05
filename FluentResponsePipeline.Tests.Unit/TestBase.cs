using Moq;

namespace FluentResponsePipeline.Tests.Unit
{
    public abstract class TestBase
    {
        protected static TObject GetMock<TObject>() 
            where TObject : class
        {
            return new Mock<TObject>().Object;
        }

        protected static TObject GetPartialMock<TObject>(params object[] args) 
            where TObject : class
        {
            return new Mock<TObject>(args) { CallBase = true }.Object;
        }
    }
}