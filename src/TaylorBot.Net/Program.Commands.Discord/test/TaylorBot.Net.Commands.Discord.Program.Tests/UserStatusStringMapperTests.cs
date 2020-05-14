using Discord;
using FluentAssertions;
using System;
using System.Collections.Generic;
using TaylorBot.Net.Commands.Discord.Program.Services;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class UserStatusStringMapperTests
    {
        private readonly UserStatusStringMapper _userStatusStringMapper = new UserStatusStringMapper();

        [Theory]
        [MemberData(nameof(GetAllEnumValues))]
        public void MapStatusToString_WhenDefinedEnumValue_ThenMapsToString(UserStatus userStatus)
        {
            Func<string> map = () => _userStatusStringMapper.MapStatusToString(userStatus);

            map.Should().NotThrow<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void MapStatusToString_WhenUndefinedEnumValue_ThenThrowsArgumentOutOfRangeException()
        {
            const UserStatus undefined = (UserStatus)(-1);
            Enum.IsDefined(typeof(UserStatus), undefined).Should().BeFalse();

            Func<string> map = () => _userStatusStringMapper.MapStatusToString(undefined);

            map.Should().Throw<ArgumentOutOfRangeException>();
        }

        public static IEnumerable<object[]> GetAllEnumValues()
        {
            foreach (var val in Enum.GetValues(typeof(UserStatus)))
            {
                yield return new object[] { val };
            }
        }
    }
}
