import {Link} from "react-router-dom";

export default function Header() {
    return (
        <div>
            <h1>Header</h1>
            <Link to="/">Home</Link>
            <Link to="/about">About</Link>
            <Link to="/contactus">Contact Us</Link>
        </div>
    )
}