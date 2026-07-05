import { useEffect } from "react";
import { useSelector, dispatch} from "react-redux";
import {authActions} from "./store";
import { useNavigate } from "react-router-dom";

function useAuthActions() {
  const navigate = useNavigate();

  async function login() {
    navigate("/login");
  }
  function logout() {
    dispatch(authActions.logout());
    navigate("/login");
  }
  return {
    login,
    logout,
  };
}


async function refreshAuthToken(refreshToken, login, logout) {
    
    const response = await fetch(
    "/api/auth/refresh",
    {
      method: "POST",

      headers: {
        "Content-Type": "application/json",
      },

      body: JSON.stringify({
        refreshToken: refreshToken,
      }),
    }
  );

  const data = await response.json();
   
  
  if(!data.ok) {
    logout();
    return;
  }
  else {
    dispatch(authActions.setToken({token: data.token, refreshToken: data.refreshToken}));
  }
}

function AuthenticateUser(email, password) {
    const {login} = useAuthActions();
    Login(email, password, login);
    useEffect(() => {
        const intervalId = setInterval(() => {
            CheckAuth();
        }, 10 * 60 * 1000);

        return () => clearInterval(intervalId);
    }, []);
}

async function Login(email, password, login) {
    const response = await fetch(
        "/api/auth/login",
        {
        method: "POST",

        headers: {
            "Content-Type": "application/json",
        },

        body: JSON.stringify({
            email,
            password,
        }),
        }
    );

    const data = await response.json();
    if(!data.ok) {
        login();
        return;
    }
    dispatch(authActions.setAuthData(data));
 
    
}

function CheckAuth() {
    const {login, logout} = useAuthActions();
    const token = useSelector((state) => state.auth.token);
    refreshAuthToken(token, login, logout);  
}
