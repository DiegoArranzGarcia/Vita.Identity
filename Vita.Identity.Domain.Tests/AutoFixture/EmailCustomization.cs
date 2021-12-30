using AutoFixture;
using AutoFixture.Kernel;
using System;
using System.Linq;
using Vita.Identity.Domain.ValueObjects;

namespace Vita.Identity.Domain.Tests.AutoFixture
{
    public class EmailCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Add(new EmailSpecimenBuilder());
        }
    }

    public class EmailSpecimenBuilder : ISpecimenBuilder
    {
        private readonly Random _rng = new();
        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public object Create(object request, ISpecimenContext context)
        {
            var requestAsType = request as Type;
            if (typeof(Email).Equals(requestAsType))
            {
                string user = new(Enumerable.Range(start: 0, count: 10)
                                            .Select(x => _chars[_rng.Next(_chars.Length)])
                                            .ToArray());

                return new Email($"{user}@test.com");
            }

            return new NoSpecimen();
        }
    }
}
