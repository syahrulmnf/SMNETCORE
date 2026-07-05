import {createSlice, configureStore} from '@reduxjs/toolkit'

const initialState = {
  value: 0,
}
const initialAuthState = {
  isAuthenticated: false,
  user: null,
  token: null,
  tenants: [],
  permissions: [],
  currentTenant: null,
}

function loadTokenFromStorage() {
  const token = localStorage.getItem("token");
  const tenants = JSON.parse(localStorage.getItem("tenants"));
  const currentTenant = JSON.parse(localStorage.getItem("currentTenant"));
  const permissions = JSON.parse(localStorage.getItem("permissions"));4
  if (token) {
    return {
      isAuthenticated: true,
      token,
      tenants,
      currentTenant,
      permissions,
    };
  } else {
    return initialAuthState;
  }
}

const authInitialState = loadTokenFromStorage();


const authReducer = createSlice({
  name: 'auth',
  authInitialState,
  reducers: {
    loginSuccess: (state, action) => {
      state.isAuthenticated = true;
      state.user = action.payload.user;
      state.token = action.payload.token;
      state.tenants = action.payload.tenants;
      state.permissions = action.payload.permissions;
      state.currentTenant = action.payload.currentTenant;
      localStorage.setItem("token", action.payload.token);
      localStorage.setItem("tenants", JSON.stringify(action.payload.tenants));
      localStorage.setItem("currentTenant", JSON.stringify(action.payload.currentTenant));
      localStorage.setItem("permissions", JSON.stringify(action.payload.permissions));
    },
    logout: (state) => {
      state.isAuthenticated = false;
      state.user = null;
      state.token = null;
      state.tenants = [];
      state.currentTenant = null;
    },
    getTenantsSuccess: (state, action) => {
      state.tenants = action.payload.tenants;
    },
    setToken: (state, action) => {
      state.token = action.payload.token;
      localStorage.setItem("token", action.payload.token);
      if(!action.refreshToken) {
        localStorage.setItem("refreshToken", action.payload.refreshToken);
      }
    }
  },
});

const globalReducer = createSlice({
  name: 'global',
  initialState,
  reducers: {
    increment: (state) => {
      state.value += 1
    },
    decrement: (state) => {
      state.value -= 1
    },
    incrementByAmount: (state, action) => {
      state.value += action.payload
    },
  },
}) 
export const globalRedActions = globalReducer.actions
export const authActions = authReducer.actions
export const globalRedStore = configureStore({
  reducer: {global: globalReducer.reducer, auth: authReducer.reducer}
});
