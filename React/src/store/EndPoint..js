// src/services/api.js

import axios from "axios";
import { useSelector } from "react-redux";

const api = axios.create({
  baseURL: "https://api.example.com",
});

api.interceptors.request.use(
  (config) => {
    const token =
      localStorage.getItem("token");

    if (token) {
      config.headers.Authorization =
        `Bearer ${token}`;
    }
    var tenants = useSelector((state) => state.auth.currentTenant);
    if(!tenants) {
      tenants = useSelector((state) => state.auth.tenants[0]);
    }
    var tenantId = tenants?.id;
    config.headers["x-tenant-id"] = tenantId;
    return config;
  },

  (error) => {
    if (error.response?.status === 401) {
      console.log("Unauthorized");
    }

    return Promise.reject(error);
  }
);

export default api;