import '../Styles/Box.scss';
import { useState } from 'react';
import axios from 'axios';
import ClipLoader from "react-spinners/ClipLoader";

export default function Box() {
    const [url, setUrl] = useState('');
    const [loading, setLoading] = useState(false);
    const [shortUrl, setShortUrl] = useState('');

    const getShortUrl = () => {
        setLoading(true);
        showLink();
        axios.post('https://linkshorter.azurewebsites.net/api/set', {
            href: url
        }).then(res => {
            console.log(res);
            let {shortLink} = res.data;
            if(shortLink) {
                setShortUrl('https://linkshorter.azurewebsites.net/api/go/' + shortLink);
                setShortUrl('https://linkshorter.azurewebsites.net/api/go/' + shortLink);
            } else {
                setShortUrl('Error');
                setShortUrl('');
            }
            setLoading(false);
        }).catch(error => {
            setLoading(false);
            setShortUrl('');
            // setShortUrl('');
        });
    }

    const showLink = () => {
        let container = document.getElementById('container');
        container.style.height = '150px';
    }

    return(
        <div id="container" className="container">
            <div className="inputs">
                <input type="text" value={url} onChange={e => setUrl(e.target.value)} placeholder="Enter your url"></input>
                <input type="submit" value="Get short url" onClick={getShortUrl}></input>
            </div>
            <div className="loadBlock">{
                (loading) ?
                <ClipLoader color="#27ff7d" loading={loading} size={25}></ClipLoader>
                :
                <a target="blank" href={shortUrl}>
                    {(shortUrl) ?
                        shortUrl
                        :
                        "Error"
                    }
                </a>
            }</div>
            
        </div>
    );
}