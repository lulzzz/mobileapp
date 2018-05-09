﻿using System;
using Toggl.Multivac;
using Toggl.Multivac.Models;

namespace Toggl.Ultrawave.ApiClients
{
    public interface IUserApi
    {
        IObservable<IUser> Get();
        IObservable<IUser> GetWithGoogle();
        IObservable<string> ResetPassword(Email email);
        IObservable<IUser> SignUp(Email email, Password password, bool termsAccepted, int countryId);
        IObservable<IUser> SignUpWithGoogle(string googleToken);
        IObservable<IUser> Update(IUser user);
    }
}
