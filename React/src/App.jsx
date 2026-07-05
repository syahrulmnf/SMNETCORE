
import { RouterProvider } from "react-router-dom";
import { router } from "./store/Router";

function App() {

  return (
    <>
      <RouterProvider router={router}/>

    </>
  )
}

export default App
