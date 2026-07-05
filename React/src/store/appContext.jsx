import { createContext, useContext, useState } from "react"; // Added useState

const AppContext = createContext();

export const AppContextProvider = ({ children }) => {
  const [appContextData, setAppContextData] = useState({
    tenant: {},
    user: {},
    permissions: [],
    credentials: {}
  });

  // Pass both the data AND the setter function
  return (
    <AppContext.Provider value={{ appContextData, setAppContextData }}>
      {children}
    </AppContext.Provider>
  );
};

export const useAppContext = () => {
  const context = useContext(AppContext);

  return context;
};
