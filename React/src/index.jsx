import ReactDOM from 'react-dom/client';
import { Provider } from 'react-redux';
import App from './App';
import { AppContextProvider } from './store/appContext';
import globalRedStore from './store/globalRedStore';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
    <Provider store={globalRedStore}>
            <AppContextProvider>
                <App />
            </AppContextProvider>
    </Provider>
);
