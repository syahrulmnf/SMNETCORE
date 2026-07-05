import {CreateBrowserRouter} from "react-router-dom";
import Home from "../pages/Home/Index";
import About from "../pages/About/Index";
import ContactUs from "../pages/ContactUs/Index";
import Galery from "../pages/Galery/Index";
import HomeLayout from "../layouts/homelayout";
import Login from "../pages/Auth/Index";
import ProtectedRoute from "./ProtectedRoute";
import AdminHome from "../pages/Admin/index";
import AdminLayout from "../layouts/AdminLayout";
import Clients from "../pages/Admin/Clients";
import ClientsSubscriptions from "../layouts/Subscriptions";
/*
const elementRoute = createRoutesFromElements(
    <Routes>
        <Route element={<HomeLayout/>}>
            <Route path="/" element={<Home/>}/>
            <Route path="/about" element={<About/>}/>
            <Route path="/contactus" element={<ContactUs/>}/>
            <Route path="/ourgalery" element={<Galery/>}/>
        </Route>
    </Routes>
)*/

export const router = CreateBrowserRouter([
    {
        path:"/login",
        element: <Login/>
    },
    {
        path: "/admin",
        element: <ProtectedRoute />,
        children:[
            {
                element: <AdminLayout />,
                children:[
                    {path: "/admin/", elemen: <AdminHome />},
                    {path: "/admin/Clients", elemen: <Clients />},
                    {path: "/admin/subscription", elemen: <ClientsSubscriptions />},
                    {path: "/admin/Clients", elemen: <Clients />},
                ]
            }]
    },
    {   path: "/",
        element: <HomeLayout/>,
        children:[
            {path: "/" , element: <Home/>},
            {path: "/about" , element: <About/>},
            {path: "/contactus" , element: <ContactUs/>},
            {path: "/ourgalery" , element: <Galery/>}
        ]
    }
]);

//export const router = CreateBrowserRouter(elementRoute)