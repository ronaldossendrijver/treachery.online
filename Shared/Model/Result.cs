// /*
//  * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
//  * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
//  * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
//  * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
//  * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
// */

namespace Treachery.Shared;

public class Result<T>
{
    public bool Success { get; init; }
    public ErrorType Error { get; init; } = ErrorType.None;
    public string ErrorDetails { get; init; }
    public T Contents { get; init; }
}

public class VoidResult : Result<VoidContents>
{
    
}

public class VoidContents;

public enum ErrorType
{
    None,
    UserNotFound,
    GameNotFound,
    ScheduledGameNotFound,
    UserNameTooShort,
    UserNameTooLong,
    UserNameExists,
    EmailExists,
    PlayerNameTooShort,
    PlayerNameTooLong,
    UserCreationFailed,
    InvalidGameVersion,
    InvalidUserNameOrPassword,
    UnknownUsernameOrEmailAddress,
    ResetRequestTooSoon,
    UnknownUserName,
    InvalidResetToken,
    ResetTokenExpired,
    NoHost,
    InvalidGameEvent,
    TooManyGames,
    TooManyScheduledGames,
    IncorrectGamePassword,
    SeatNotAvailableNotOpen,
    SeatNotAvailableKicked,
    AlreadyObserver,
    CannotRemoveLastHost,
    NoCreator,
    AlreadyPlayer,
    UserNotInGame
}