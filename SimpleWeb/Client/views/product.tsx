import * as React from "react";
import {Dispatch} from "redux";
import {connect} from "react-redux";

import {removeProduct} from "../store/products";
import {IProduct, Products} from "../api/products";
import publisher from "../utils/publisher";

import {ActionProp} from "./common";

interface IProps {
    product: IProduct;
}
interface IDispatchProps {
    onRemove: ActionProp<IProduct>;
}
class Product extends React.Component<IProps & IDispatchProps, {}> {
    render() {
        const onRemove = this.props.onRemove.bind(this, this.props.product);
        return <div>
            <span>{this.props.product.name} </span>
            <button onClick={onRemove}>&times;</button>
        </div>;
    }
}

const dispatchMap = (dispatch: Dispatch<any>) => ({
    onRemove: (p: IProduct): any =>
        publisher.publish(removeProduct(p))
    //onRemove: (p: IProduct): any =>
        //Products.remove(p).then(() => dispatch(removeProduct(p)))
});
export default connect<{}, IDispatchProps, IProps>(null, dispatchMap)(Product);